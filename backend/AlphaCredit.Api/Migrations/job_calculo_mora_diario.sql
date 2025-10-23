-- =============================================================================
-- JOB: Cálculo Diario de Mora para Préstamos
-- Descripción: Genera componentes de MORA diariamente para cuotas vencidas
--              usando la fecha del sistema (tabla fechasistema)
-- =============================================================================

-- PRERREQUISITOS:
-- 1. Extensión pg_cron instalada (requiere superusuario)
--    CREATE EXTENSION IF NOT EXISTS pg_cron;
--
-- 2. Componente de tipo MORA debe existir en componenteprestamo
--    INSERT INTO componenteprestamo (componenteprestamonombre, componenteprestamotipo, componenteprestamoestaactivo)
--    VALUES ('Mora', 'MORA', true);

-- =============================================================================
-- FUNCIÓN PRINCIPAL: Genera componentes de mora diaria
-- =============================================================================

CREATE OR REPLACE FUNCTION fn_generar_mora_diaria()
RETURNS TABLE (
    prestamosafectados INTEGER,
    componentesgenerados INTEGER,
    fechacalculo TIMESTAMP,
    mensaje TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_fecha_sistema TIMESTAMP;
    v_tasa_mora_diaria DECIMAL(10,8);
    v_componente_mora_id BIGINT;
    v_estado_pendiente_id BIGINT;
    v_prestamos_afectados INTEGER := 0;
    v_componentes_generados INTEGER := 0;
    v_prestamo RECORD;
    v_componente RECORD;
    v_mora_diaria DECIMAL(18,2);
    v_dias_vencidos INTEGER;
BEGIN
    -- 1. Obtener fecha del sistema
    SELECT fechasistemafecha
    INTO v_fecha_sistema
    FROM fechasistema
    WHERE fechasistemaestaactiva = true
    ORDER BY fechasistemafecha DESC
    LIMIT 1;

    IF v_fecha_sistema IS NULL THEN
        v_fecha_sistema := CURRENT_DATE;
    END IF;

    -- 2. Obtener tasa de mora diaria (3% mensual / 30 días = 0.1% diario)
    SELECT CAST(parametrossistemavalor AS DECIMAL(10,4)) / 30 / 100
    INTO v_tasa_mora_diaria
    FROM parametrossistema
    WHERE parametrosistemallave = 'TASA_MORA_MENSUAL';

    IF v_tasa_mora_diaria IS NULL THEN
        v_tasa_mora_diaria := 3.0 / 30 / 100; -- 3% mensual por defecto = 0.1% diario
    END IF;

    -- 3. Obtener ID de componente MORA
    SELECT componenteprestamoid
    INTO v_componente_mora_id
    FROM componenteprestamo
    WHERE componenteprestamotipo = 'MORA'
      AND componenteprestamoestaactivo = true
    LIMIT 1;

    IF v_componente_mora_id IS NULL THEN
        RAISE EXCEPTION 'No se encontró componente de tipo MORA activo';
    END IF;

    -- 4. Obtener ID de estado PENDIENTE
    SELECT estadocomponenteid
    INTO v_estado_pendiente_id
    FROM estadocomponente
    WHERE UPPER(estadocomponentenombre) = 'PENDIENTE'
    LIMIT 1;

    IF v_estado_pendiente_id IS NULL THEN
        RAISE EXCEPTION 'No se encontró estado PENDIENTE';
    END IF;

    -- 5. Procesar cada préstamo activo con cuotas vencidas
    FOR v_prestamo IN
        SELECT DISTINCT p.prestamoid
        FROM prestamo p
        INNER JOIN prestamocomponente pc ON p.prestamoid = pc.prestamoid
        WHERE p.estadoprestamoid != 3 -- No cancelados (ajustar según tu catálogo)
          AND pc.prestamocomponentefechavencimiento < v_fecha_sistema
          AND pc.prestamocomponentesaldo > 0
    LOOP
        -- 6. Procesar componentes vencidos de este préstamo
        FOR v_componente IN
            SELECT pc.*,
                   cp.componenteprestamotipo,
                   EXTRACT(DAY FROM (v_fecha_sistema - pc.prestamocomponentefechavenimiento)) AS diasvencidos
            FROM prestamocomponente pc
            INNER JOIN componenteprestamo cp ON pc.componenteprestamoid = cp.componenteprestamoid
            WHERE pc.prestamoid = v_prestamo.prestamoid
              AND pc.prestamocomponentefechavencimiento < v_fecha_sistema
              AND pc.prestamocomponentesaldo > 0
              AND cp.componenteprestamotipo IN ('CAPITAL', 'INTERES')
        LOOP
            v_dias_vencidos := EXTRACT(DAY FROM (v_fecha_sistema - v_componente.prestamocomponentefechavencimiento));

            IF v_dias_vencidos > 0 THEN
                -- Calcular mora del día actual (no acumulativa)
                v_mora_diaria := ROUND(v_componente.prestamocomponentesaldo * v_tasa_mora_diaria, 2);

                IF v_mora_diaria > 0 THEN
                    -- Verificar si ya existe componente de mora para este día
                    IF NOT EXISTS (
                        SELECT 1
                        FROM prestamocomponente
                        WHERE prestamoid = v_prestamo.prestamoid
                          AND componenteprestamoid = v_componente_mora_id
                          AND prestamocomponentenumerocuota = v_componente.prestamocomponentenumerocuota
                          AND DATE(prestamocomponentefechavencimiento) = DATE(v_fecha_sistema)
                    ) THEN
                        -- Crear componente de mora para hoy
                        INSERT INTO prestamocomponente (
                            prestamoid,
                            componenteprestamoid,
                            estadocomponenteid,
                            prestamocomponentemonto,
                            prestamocomponentesaldo,
                            prestamocomponentefechavencimiento,
                            prestamocomponentenumerocuota
                        ) VALUES (
                            v_prestamo.prestamoid,
                            v_componente_mora_id,
                            v_estado_pendiente_id,
                            v_mora_diaria,
                            v_mora_diaria,
                            v_fecha_sistema,
                            v_componente.prestamocomponentenumerocuota
                        );

                        v_componentes_generados := v_componentes_generados + 1;
                    END IF;
                END IF;
            END IF;
        END LOOP;

        -- 7. Actualizar saldo de mora del préstamo
        UPDATE prestamo
        SET prestamosaldomora = (
            SELECT COALESCE(SUM(prestamocomponentesaldo), 0)
            FROM prestamocomponente pc
            INNER JOIN componenteprestamo cp ON pc.componenteprestamoid = cp.componenteprestamoid
            WHERE pc.prestamoid = v_prestamo.prestamoid
              AND cp.componenteprestamotipo = 'MORA'
              AND pc.prestamocomponentesaldo > 0
        ),
        prestamofechamodifica = v_fecha_sistema
        WHERE prestamoid = v_prestamo.prestamoid;

        v_prestamos_afectados := v_prestamos_afectados + 1;
    END LOOP;

    -- 8. Retornar resultados
    RETURN QUERY SELECT
        v_prestamos_afectados,
        v_componentes_generados,
        v_fecha_sistema,
        FORMAT('Procesados %s préstamos, generados %s componentes de mora',
               v_prestamos_afectados, v_componentes_generados);
END;
$$;

COMMENT ON FUNCTION fn_generar_mora_diaria IS 'Genera componentes de mora diaria para préstamos con cuotas vencidas usando la fecha del sistema';

-- =============================================================================
-- PROGRAMAR JOB DIARIO (usando pg_cron)
-- =============================================================================

-- NOTA: Esto requiere la extensión pg_cron y permisos de superusuario
-- Si no tienes pg_cron, puedes ejecutar esta función manualmente o desde tu aplicación

-- Opción 1: Con pg_cron (requiere extensión instalada)
/*
SELECT cron.schedule(
    'calculo-mora-diario',           -- Nombre del job
    '0 0 * * *',                     -- Cron: Todos los días a las 00:00
    $$SELECT * FROM fn_generar_mora_diaria()$$
);
*/

-- Opción 2: Crear tabla de log para ejecutar desde la aplicación
CREATE TABLE IF NOT EXISTS log_calculo_mora (
    logid BIGSERIAL PRIMARY KEY,
    logfechaejecucion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    logprestamosafectados INTEGER,
    logcomponentesgenerados INTEGER,
    logfechacalculo TIMESTAMP,
    logmensaje TEXT,
    logduracionms INTEGER
);

-- =============================================================================
-- FUNCIÓN WRAPPER para logging
-- =============================================================================

CREATE OR REPLACE FUNCTION fn_ejecutar_calculo_mora_con_log()
RETURNS VOID
LANGUAGE plpgsql
AS $$
DECLARE
    v_inicio TIMESTAMP;
    v_fin TIMESTAMP;
    v_duracion INTEGER;
    v_resultado RECORD;
BEGIN
    v_inicio := CLOCK_TIMESTAMP();

    -- Ejecutar cálculo de mora
    SELECT * INTO v_resultado
    FROM fn_generar_mora_diaria()
    LIMIT 1;

    v_fin := CLOCK_TIMESTAMP();
    v_duracion := EXTRACT(MILLISECONDS FROM (v_fin - v_inicio));

    -- Registrar en log
    INSERT INTO log_calculo_mora (
        logprestamosafectados,
        logcomponentesgenerados,
        logfechacalculo,
        logmensaje,
        logduracionms
    ) VALUES (
        v_resultado.prestamosafectados,
        v_resultado.componentesgenerados,
        v_resultado.fechacalculo,
        v_resultado.mensaje,
        v_duracion
    );
END;
$$;

-- =============================================================================
-- EJECUCIÓN MANUAL (para pruebas)
-- =============================================================================

-- Ejecutar cálculo de mora inmediatamente
-- SELECT * FROM fn_generar_mora_diaria();

-- Ejecutar con logging
-- SELECT fn_ejecutar_calculo_mora_con_log();

-- Ver log de ejecuciones
-- SELECT * FROM log_calculo_mora ORDER BY logfechaejecucion DESC LIMIT 10;

-- =============================================================================
-- INSTRUCCIONES DE USO
-- =============================================================================

/*
1. EJECUTAR DESDE LA APLICACIÓN (.NET):
   - Crear un endpoint API que ejecute fn_generar_mora_diaria()
   - Llamar este endpoint diariamente usando un servicio de tareas programadas

2. EJECUTAR CON PG_CRON (si está disponible):
   - Descomentar el bloque SELECT cron.schedule(...) de arriba
   - El job se ejecutará automáticamente todos los días a medianoche

3. EJECUTAR MANUALMENTE:
   - SELECT * FROM fn_generar_mora_diaria();

4. VERIFICAR RESULTADOS:
   - SELECT * FROM log_calculo_mora ORDER BY logfechaejecucion DESC;

5. CANCELAR JOB (si usas pg_cron):
   - SELECT cron.unschedule('calculo-mora-diario');
*/
