-- Script para generar un préstamo con una cuota que vence HOY
-- Fecha de ejecución: 2025-10-23

-- ============================================================================
-- CONFIGURACIÓN DEL PRÉSTAMO
-- ============================================================================
-- Monto: 10,000.00
-- Tasa de interés: 24% anual
-- Plazo: 12 cuotas mensuales
-- Frecuencia de pago: Mensual (30 días)
-- Cuota que vence hoy: Cuota #3 (vence 2025-10-23)
-- ============================================================================

-- Asegurar que no hay transacción pendiente
ROLLBACK;

BEGIN;

-- Variables para el préstamo
DO $$
DECLARE
    v_empresa_id BIGINT;
    v_sucursal_id BIGINT;
    v_persona_id BIGINT;
    v_moneda_id BIGINT;
    v_estado_prestamo_vigente_id BIGINT;
    v_frecuencia_pago_mensual_id BIGINT;
    v_destino_credito_id BIGINT;
    v_prestamo_id BIGINT;
    v_prestamo_numero VARCHAR(20);
    v_componente_capital_id BIGINT;
    v_componente_interes_id BIGINT;
    v_estado_componente_pendiente_id BIGINT;
    v_monto_prestamo DECIMAL(18,2);
    v_tasa_interes DECIMAL(5,2);
    v_plazo INT;
    v_fecha_desembolso DATE;
    v_fecha_hoy DATE;
    v_tasa_mensual DECIMAL(18,10);
    v_cuota_fija DECIMAL(18,2);
    v_saldo_capital DECIMAL(18,2);
    v_fecha_pago DATE;
    v_interes_cuota DECIMAL(18,2);
    v_capital_cuota DECIMAL(18,2);
    v_cuota_numero INT;
BEGIN
    -- Inicializar parámetros del préstamo
    v_monto_prestamo := 10000.00;
    v_tasa_interes := 24.00;
    v_plazo := 12;
    v_fecha_desembolso := '2025-07-23'::DATE;
    v_fecha_hoy := CURRENT_DATE;

    RAISE NOTICE 'Iniciando creacion de prestamo...';

    -- Obtener IDs necesarios
    SELECT empresaid INTO v_empresa_id FROM empresa WHERE empresaestaactiva = true LIMIT 1;
    IF v_empresa_id IS NULL THEN
        RAISE EXCEPTION 'No se encontro una empresa activa';
    END IF;
    RAISE NOTICE 'Empresa ID: %', v_empresa_id;

    SELECT sucursalid INTO v_sucursal_id FROM sucursal WHERE empresaid = v_empresa_id AND sucursalestaactiva = true LIMIT 1;
    IF v_sucursal_id IS NULL THEN
        RAISE EXCEPTION 'No se encontro una sucursal activa';
    END IF;
    RAISE NOTICE 'Sucursal ID: %', v_sucursal_id;

    SELECT monedaid INTO v_moneda_id FROM moneda WHERE monedacodigo = 'NIO' LIMIT 1;
    IF v_moneda_id IS NULL THEN
        SELECT monedaid INTO v_moneda_id FROM moneda LIMIT 1;
    END IF;
    RAISE NOTICE 'Moneda ID: %', v_moneda_id;

    SELECT estadoprestamoid INTO v_estado_prestamo_vigente_id FROM estadoprestamo WHERE estadoprestamonombre = 'Vigente' LIMIT 1;
    IF v_estado_prestamo_vigente_id IS NULL THEN
        SELECT estadoprestamoid INTO v_estado_prestamo_vigente_id FROM estadoprestamo LIMIT 1;
    END IF;
    RAISE NOTICE 'Estado Prestamo ID: %', v_estado_prestamo_vigente_id;

    SELECT frecuenciapagoid INTO v_frecuencia_pago_mensual_id FROM frecuenciapago WHERE frecuenciapagofrec = 30 LIMIT 1;
    IF v_frecuencia_pago_mensual_id IS NULL THEN
        SELECT frecuenciapagoid INTO v_frecuencia_pago_mensual_id FROM frecuenciapago LIMIT 1;
    END IF;
    RAISE NOTICE 'Frecuencia Pago ID: %', v_frecuencia_pago_mensual_id;

    SELECT destinocreditoid INTO v_destino_credito_id FROM destinocredito LIMIT 1;

    -- Obtener o crear persona de prueba
    SELECT personaid INTO v_persona_id FROM persona WHERE personacedula = '999-999999-9999X' LIMIT 1;

    IF v_persona_id IS NULL THEN
        RAISE NOTICE 'Creando persona de prueba...';
        INSERT INTO persona (
            tipopersonaid,
            personanombres,
            personaapellidos,
            personacedula,
            personadireccion,
            personatelefono,
            personaemail,
            personafechanacimiento,
            personaactivo,
            personafechacreacion
        )
        SELECT
            tipopersonaid,
            'CLIENTE PRUEBA VENCE HOY',
            'SCRIPT GENERADO',
            '999-999999-9999X',
            'Direccion de prueba',
            '99999999',
            'prueba.hoy@alphatest.com',
            '1990-01-01'::DATE,
            true,
            CURRENT_TIMESTAMP
        FROM tipopersona
        WHERE tipopersonanombre = 'Natural'
        LIMIT 1
        RETURNING personaid INTO v_persona_id;
    END IF;
    RAISE NOTICE 'Persona ID: %', v_persona_id;

    -- Obtener IDs de componentes
    SELECT componenteprestamoid INTO v_componente_capital_id FROM componenteprestamo WHERE componenteprestamonombre = 'Capital' LIMIT 1;
    SELECT componenteprestamoid INTO v_componente_interes_id FROM componenteprestamo WHERE componenteprestamonombre = 'Interes' LIMIT 1;

    IF v_componente_interes_id IS NULL THEN
        SELECT componenteprestamoid INTO v_componente_interes_id FROM componenteprestamo WHERE componenteprestamonombre LIKE 'Inter%' LIMIT 1;
    END IF;

    SELECT estadocomponenteid INTO v_estado_componente_pendiente_id FROM estadocomponente WHERE estadocomponentenombre = 'Pendiente' LIMIT 1;

    RAISE NOTICE 'Componente Capital ID: %', v_componente_capital_id;
    RAISE NOTICE 'Componente Interes ID: %', v_componente_interes_id;
    RAISE NOTICE 'Estado Componente ID: %', v_estado_componente_pendiente_id;

    -- Generar número de préstamo único
    v_prestamo_numero := 'PRE-HOY-' || TO_CHAR(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISS');

    -- Calcular tasa mensual
    v_tasa_mensual := (v_tasa_interes / 12 / 100);

    -- Calcular cuota fija usando fórmula francesa
    v_cuota_fija := ROUND(
        v_monto_prestamo * (v_tasa_mensual * POWER(1 + v_tasa_mensual, v_plazo)) /
        (POWER(1 + v_tasa_mensual, v_plazo) - 1),
        2
    );

    RAISE NOTICE 'Cuota fija calculada: %', v_cuota_fija;

    -- Insertar préstamo
    INSERT INTO prestamo (
        personaid,
        sucursalid,
        monedaid,
        estadoprestamoid,
        destinocreditoid,
        frecuenciapagoid,
        prestamonumero,
        prestamomonto,
        prestamotasainteres,
        prestamoplazo,
        prestamofechaaprobacion,
        prestamofechadesembolso,
        prestamofechavencimiento,
        prestamosaldocapital,
        prestamosaldointeres,
        prestamosaldomora,
        prestamoobservaciones,
        prestamousercrea,
        prestamofechacreacion
    )
    VALUES (
        v_persona_id,
        v_sucursal_id,
        v_moneda_id,
        v_estado_prestamo_vigente_id,
        v_destino_credito_id,
        v_frecuencia_pago_mensual_id,
        v_prestamo_numero,
        v_monto_prestamo,
        v_tasa_interes,
        v_plazo,
        v_fecha_desembolso,
        v_fecha_desembolso,
        v_fecha_desembolso + INTERVAL '12 months',
        v_monto_prestamo,
        0,
        0,
        'Prestamo de prueba con cuota que vence hoy',
        'SCRIPT',
        CURRENT_TIMESTAMP
    )
    RETURNING prestamoid INTO v_prestamo_id;

    RAISE NOTICE 'Prestamo creado con ID: %', v_prestamo_id;
    RAISE NOTICE 'Numero de prestamo: %', v_prestamo_numero;

    -- Generar tabla de amortización y componentes
    v_saldo_capital := v_monto_prestamo;

    FOR v_cuota_numero IN 1..v_plazo LOOP
        -- Calcular fecha de pago
        v_fecha_pago := v_fecha_desembolso + (v_cuota_numero * INTERVAL '30 days');

        -- Calcular interés de la cuota
        v_interes_cuota := ROUND(v_saldo_capital * v_tasa_mensual, 2);

        -- Calcular capital de la cuota
        v_capital_cuota := v_cuota_fija - v_interes_cuota;

        -- Ajustar última cuota si hay diferencias por redondeo
        IF v_cuota_numero = v_plazo THEN
            v_capital_cuota := v_saldo_capital;
            v_interes_cuota := v_cuota_fija - v_capital_cuota;
        END IF;

        -- Insertar componente de Capital
        INSERT INTO prestamocomponente (
            prestamoid,
            componenteprestamoid,
            estadocomponenteid,
            prestamocomponentemonto,
            prestamocomponentesaldo,
            prestamocomponentefechavencimiento,
            prestamocomponentenumerocuota
        )
        VALUES (
            v_prestamo_id,
            v_componente_capital_id,
            v_estado_componente_pendiente_id,
            v_capital_cuota,
            v_capital_cuota,
            v_fecha_pago,
            v_cuota_numero
        );

        -- Insertar componente de Interés
        INSERT INTO prestamocomponente (
            prestamoid,
            componenteprestamoid,
            estadocomponenteid,
            prestamocomponentemonto,
            prestamocomponentesaldo,
            prestamocomponentefechavencimiento,
            prestamocomponentenumerocuota
        )
        VALUES (
            v_prestamo_id,
            v_componente_interes_id,
            v_estado_componente_pendiente_id,
            v_interes_cuota,
            v_interes_cuota,
            v_fecha_pago,
            v_cuota_numero
        );

        -- Actualizar saldo de capital
        v_saldo_capital := v_saldo_capital - v_capital_cuota;
    END LOOP;

    RAISE NOTICE '========================================';
    RAISE NOTICE 'PRESTAMO GENERADO EXITOSAMENTE';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'ID Prestamo: %', v_prestamo_id;
    RAISE NOTICE 'Numero: %', v_prestamo_numero;
    RAISE NOTICE 'Cliente: CLIENTE PRUEBA VENCE HOY';
    RAISE NOTICE 'Monto: %', v_monto_prestamo;
    RAISE NOTICE 'Tasa anual: %', v_tasa_interes;
    RAISE NOTICE 'Cuota mensual: %', v_cuota_fija;
    RAISE NOTICE 'Fecha desembolso: %', v_fecha_desembolso;
    RAISE NOTICE 'CUOTA 3 VENCE HOY: %', v_fecha_hoy;
    RAISE NOTICE '========================================';

END $$;

COMMIT;

-- Verificar el préstamo creado
SELECT
    p.prestamoid,
    p.prestamonumero,
    per.personanombres || ' ' || per.personaapellidos as cliente,
    p.prestamomonto as monto,
    p.prestamotasainteres as tasa,
    p.prestamofechadesembolso as fecha_desembolso,
    p.prestamoplazo as plazo,
    ep.estadoprestamonombre as estado
FROM prestamo p
JOIN persona per ON p.personaid = per.personaid
JOIN estadoprestamo ep ON p.estadoprestamoid = ep.estadoprestamoid
WHERE p.prestamonumero LIKE 'PRE-HOY-%'
ORDER BY p.prestamoid DESC
LIMIT 1;

-- Verificar las cuotas y destacar la que vence hoy
SELECT
    pc.prestamocomponentenumerocuota as cuota,
    pc.prestamocomponentefechavencimiento as fecha_vencimiento,
    cp.componenteprestamonombre as tipo,
    pc.prestamocomponentemonto as monto,
    pc.prestamocomponentesaldo as saldo,
    ec.estadocomponentenombre as estado,
    CASE
        WHEN pc.prestamocomponentefechavencimiento = CURRENT_DATE THEN '*** VENCE HOY ***'
        ELSE ''
    END as alerta
FROM prestamocomponente pc
JOIN componenteprestamo cp ON pc.componenteprestamoid = cp.componenteprestamoid
JOIN estadocomponente ec ON pc.estadocomponenteid = ec.estadocomponenteid
WHERE pc.prestamoid = (
    SELECT prestamoid
    FROM prestamo
    WHERE prestamonumero LIKE 'PRE-HOY-%'
    ORDER BY prestamoid DESC
    LIMIT 1
)
ORDER BY pc.prestamocomponentenumerocuota, cp.componenteprestamonombre;
