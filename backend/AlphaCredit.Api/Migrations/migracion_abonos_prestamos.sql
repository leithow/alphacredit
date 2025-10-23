-- =============================================================================
-- MIGRACIÓN: Sistema de Abonos a Préstamos
-- Descripción: Agrega soporte completo para abonos a préstamos con integración
--              a fondos, incluyendo cuotas completas, parciales, abonos a capital
--              y pagos de mora
-- =============================================================================

-- 1. Agregar relación de Forma de Pago con Fondo
ALTER TABLE formapago
ADD COLUMN fondoid BIGINT;

ALTER TABLE formapago
ADD CONSTRAINT fk_formapago_fondo
FOREIGN KEY (fondoid) REFERENCES fondo(fondoid) ON DELETE SET NULL;

COMMENT ON COLUMN formapago.fondoid IS 'Fondo al que se dirigen los pagos realizados con esta forma de pago';

-- 2. Crear tabla PagoDetalle para registro detallado de abonos
CREATE SEQUENCE pagodetalleid;

CREATE TABLE pagodetalle (
    pagodetalleid BIGINT NOT NULL DEFAULT NEXTVAL('pagodetalleid'),
    movimientoprestamoid BIGINT NOT NULL,
    prestamocomponenteid BIGINT,
    componenteprestamoid BIGINT NOT NULL,
    pagodetallecuotanumero INTEGER,
    pagodetallemontoaplicado DECIMAL(18,2) NOT NULL,
    pagodetallemontoantes DECIMAL(18,2) NOT NULL,
    pagodetallefechaaplicacion TIMESTAMP NOT NULL,

    CONSTRAINT pk_pagodetalle PRIMARY KEY (pagodetalleid),
    CONSTRAINT fk_pagodetalle_movimientoprestamo FOREIGN KEY (movimientoprestamoid)
        REFERENCES movimientoprestamo(movimientoprestamoid) ON DELETE CASCADE,
    CONSTRAINT fk_pagodetalle_prestamocomponente FOREIGN KEY (prestamocomponenteid)
        REFERENCES prestamocomponente(prestamocomponenteid) ON DELETE SET NULL,
    CONSTRAINT fk_pagodetalle_componenteprestamo FOREIGN KEY (componenteprestamoid)
        REFERENCES componenteprestamo(componenteprestamoid) ON DELETE RESTRICT
);

-- Índices para mejorar rendimiento de consultas
CREATE INDEX idx_pagodetalle_movimiento ON pagodetalle(movimientoprestamoid);
CREATE INDEX idx_pagodetalle_componente ON pagodetalle(prestamocomponenteid);
CREATE INDEX idx_pagodetalle_cuota ON pagodetalle(pagodetallecuotanumero);
CREATE INDEX idx_pagodetalle_fecha ON pagodetalle(pagodetallefechaaplicacion);

-- Comentarios de la tabla
COMMENT ON TABLE pagodetalle IS 'Detalle de pagos aplicados a componentes específicos de cuotas de préstamos';
COMMENT ON COLUMN pagodetalle.pagodetalleid IS 'Identificador único del detalle de pago';
COMMENT ON COLUMN pagodetalle.movimientoprestamoid IS 'Movimiento de préstamo al que pertenece este detalle';
COMMENT ON COLUMN pagodetalle.prestamocomponenteid IS 'Componente de préstamo al que se aplicó el pago';
COMMENT ON COLUMN pagodetalle.componenteprestamoid IS 'Tipo de componente (capital, interés, mora)';
COMMENT ON COLUMN pagodetalle.pagodetallecuotanumero IS 'Número de cuota pagada';
COMMENT ON COLUMN pagodetalle.pagodetallemontoaplicado IS 'Monto aplicado en este pago';
COMMENT ON COLUMN pagodetalle.pagodetallemontoantes IS 'Saldo del componente antes del pago';
COMMENT ON COLUMN pagodetalle.pagodetallefechaaplicacion IS 'Fecha en que se aplicó el pago';

-- 3. Insertar parámetro de sistema para tasa de mora (si no existe)
-- TASA: 3% mensual = 36% anual = 0.1% diario
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM parametrossistema WHERE parametrosistemallave = 'TASA_MORA_MENSUAL') THEN
        INSERT INTO parametrossistema (parametrosistemallave, parametrossistemadescripcion, parametrossistemavalor, parametrossistemaestaactivo)
        VALUES ('TASA_MORA_MENSUAL', 'Tasa de mora mensual aplicada a cuotas vencidas (%)', '3.0', true);
    END IF;
END $$;

-- 4. Insertar componente de tipo MORA (si no existe)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM componenteprestamo WHERE componenteprestamotipo = 'MORA') THEN
        INSERT INTO componenteprestamo (componenteprestamonombre, componenteprestamodescripcion, componenteprestamotipo, componenteprestamoestaactivo)
        VALUES ('Mora', 'Mora por retraso en pagos', 'MORA', true);
    END IF;
END $$;

-- 5. Insertar estados de componente necesarios (si no existen)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM estadocomponente WHERE UPPER(estadocomponentenombre) = 'PENDIENTE') THEN
        INSERT INTO estadocomponente (estadocomponentenombre, estadocomponentedescripcion, estadocomponenteestaactivo)
        VALUES ('PENDIENTE', 'Componente pendiente de pago', true);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM estadocomponente WHERE UPPER(estadocomponentenombre) = 'PARCIAL') THEN
        INSERT INTO estadocomponente (estadocomponentenombre, estadocomponentedescripcion, estadocomponenteestaactivo)
        VALUES ('PARCIAL', 'Componente parcialmente pagado', true);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM estadocomponente WHERE UPPER(estadocomponentenombre) = 'PAGADO') THEN
        INSERT INTO estadocomponente (estadocomponentenombre, estadocomponentedescripcion, estadocomponenteestaactivo)
        VALUES ('PAGADO', 'Componente totalmente pagado', true);
    END IF;
END $$;

-- 5. Crear vista para estado de cuenta de préstamos
CREATE OR REPLACE VIEW v_estado_cuenta_prestamos AS
SELECT
    p.prestamoid,
    p.prestamonumero,
    per.personaprimernombre || ' ' || per.personaprimerapellido AS nombrecliente,
    p.prestamomonto AS montooriginal,
    p.prestamosaldocapital AS saldocapital,
    p.prestamosaldointeres AS saldointeres,
    p.prestamosaldomora AS saldomorphacienda,
    (p.prestamosaldocapital + p.prestamosaldointeres + p.prestamosaldomora) AS saldototal,
    p.prestamofechadesembolso,
    p.prestamofechavencimiento,
    ep.estadoprestamonombre AS estadoprestamo,
    COUNT(DISTINCT pc.prestamocomponentenumerocuota) AS totalcuotas,
    COUNT(DISTINCT CASE WHEN ec.estadocomponentenombre = 'PAGADO' AND cp.componenteprestamotipo = 'CAPITAL'
                        THEN pc.prestamocomponentenumerocuota END) AS cuotaspagadas,
    COUNT(DISTINCT CASE WHEN ec.estadocomponentenombre != 'PAGADO' AND cp.componenteprestamotipo = 'CAPITAL'
                        THEN pc.prestamocomponentenumerocuota END) AS cuotaspendientes
FROM prestamo p
INNER JOIN persona per ON p.personaid = per.personaid
INNER JOIN estadoprestamo ep ON p.estadoprestamoid = ep.estadoprestamoid
LEFT JOIN prestamocomponente pc ON p.prestamoid = pc.prestamoid
LEFT JOIN componenteprestamo cp ON pc.componenteprestamoid = cp.componenteprestamoid
LEFT JOIN estadocomponente ec ON pc.estadocomponenteid = ec.estadocomponenteid
GROUP BY p.prestamoid, p.prestamonumero, per.personaprimernombre, per.personaprimerapellido,
         p.prestamomonto, p.prestamosaldocapital, p.prestamosaldointeres, p.prestamosaldomora,
         p.prestamofechadesembolso, p.prestamofechavencimiento, ep.estadoprestamonombre;

COMMENT ON VIEW v_estado_cuenta_prestamos IS 'Vista resumen del estado de cuenta de préstamos';

-- 6. Crear función para calcular mora de un préstamo
CREATE OR REPLACE FUNCTION fn_calcular_mora_prestamo(
    p_prestamoid BIGINT,
    p_fechacalculo TIMESTAMP DEFAULT CURRENT_TIMESTAMP
)
RETURNS DECIMAL(18,2)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tasa_mora_anual DECIMAL(10,4);
    v_tasa_mora_diaria DECIMAL(10,8);
    v_mora_total DECIMAL(18,2) := 0;
    v_componente RECORD;
BEGIN
    -- Obtener tasa de mora
    SELECT CAST(parametrossistemavalor AS DECIMAL(10,4)) / 365 / 100
    INTO v_tasa_mora_diaria
    FROM parametrossistema
    WHERE parametrosistemallave = 'TASA_MORA_ANUAL';

    IF v_tasa_mora_diaria IS NULL THEN
        v_tasa_mora_diaria := 0.24 / 365; -- Valor por defecto 24% anual
    END IF;

    -- Calcular mora para cada componente vencido
    FOR v_componente IN
        SELECT prestamocomponentesaldo,
               prestamocomponentefechavencimiento,
               EXTRACT(DAY FROM (p_fechacalculo - prestamocomponentefechavencimiento)) AS diasvencidos
        FROM prestamocomponente
        WHERE prestamoid = p_prestamoid
          AND prestamocomponentefechavencimiento < p_fechacalculo
          AND prestamocomponentesaldo > 0
    LOOP
        IF v_componente.diasvencidos > 0 THEN
            v_mora_total := v_mora_total +
                           (v_componente.prestamocomponentesaldo * v_tasa_mora_diaria * v_componente.diasvencidos);
        END IF;
    END LOOP;

    RETURN ROUND(v_mora_total, 2);
END;
$$;

COMMENT ON FUNCTION fn_calcular_mora_prestamo IS 'Calcula la mora total de un préstamo a una fecha específica';

-- 7. Otorgar permisos (ajustar según el usuario de tu aplicación)
-- GRANT SELECT, INSERT, UPDATE, DELETE ON pagodetalle TO tu_usuario_app;
-- GRANT SELECT ON v_estado_cuenta_prestamos TO tu_usuario_app;
-- GRANT EXECUTE ON FUNCTION fn_calcular_mora_prestamo TO tu_usuario_app;

-- =============================================================================
-- FIN DE LA MIGRACIÓN
-- =============================================================================

-- Verificación de la migración
SELECT 'Migración completada exitosamente' AS status,
       (SELECT COUNT(*) FROM information_schema.columns
        WHERE table_name = 'formapago' AND column_name = 'fondoid') AS formapago_fondoid_agregado,
       (SELECT COUNT(*) FROM information_schema.tables
        WHERE table_name = 'pagodetalle') AS tabla_pagodetalle_creada,
       (SELECT COUNT(*) FROM information_schema.views
        WHERE table_name = 'v_estado_cuenta_prestamos') AS vista_estado_cuenta_creada,
       (SELECT COUNT(*) FROM information_schema.routines
        WHERE routine_name = 'fn_calcular_mora_prestamo') AS funcion_mora_creada;
