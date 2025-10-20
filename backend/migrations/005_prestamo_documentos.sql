-- Migración: Tablas para documentos de préstamos y control de impresiones
-- Fecha: 2025-10-19
-- Descripción: Crea las tablas para gestionar documentos de préstamos (contrato, pagaré, plan de pagos)
--              y el control de impresiones de cada documento

-- Tabla: prestamo_documento
-- Almacena los documentos generados para cada préstamo
CREATE TABLE IF NOT EXISTS prestamo_documento (
    prestamo_documentoid BIGSERIAL PRIMARY KEY,
    prestamoid BIGINT NOT NULL,
    prestamo_documento_tipo VARCHAR(50) NOT NULL, -- CONTRATO, PAGARE, PLAN_PAGOS
    prestamo_documento_ruta VARCHAR(500),
    prestamo_documento_hash VARCHAR(100), -- Para verificar integridad del documento
    prestamo_documento_veces_impreso INT DEFAULT 0 NOT NULL,
    prestamo_documento_fecha_primera_impresion TIMESTAMP,
    prestamo_documento_fecha_ultima_impresion TIMESTAMP,
    prestamo_documento_user_crea VARCHAR(40),
    prestamo_documento_fecha_creacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,

    CONSTRAINT fk_prestamo_documento_prestamo
        FOREIGN KEY (prestamoid)
        REFERENCES prestamo(prestamoid)
        ON DELETE CASCADE,

    CONSTRAINT uk_prestamo_documento
        UNIQUE (prestamoid, prestamo_documento_tipo)
);

-- Índices para prestamo_documento
CREATE INDEX idx_prestamo_documento_prestamoid ON prestamo_documento(prestamoid);
CREATE INDEX idx_prestamo_documento_tipo ON prestamo_documento(prestamo_documento_tipo);

-- Comentarios
COMMENT ON TABLE prestamo_documento IS 'Documentos generados para préstamos (contratos, pagarés, planes de pago)';
COMMENT ON COLUMN prestamo_documento.prestamo_documento_tipo IS 'Tipo de documento: CONTRATO, PAGARE, PLAN_PAGOS';
COMMENT ON COLUMN prestamo_documento.prestamo_documento_hash IS 'Hash del documento para verificar integridad';
COMMENT ON COLUMN prestamo_documento.prestamo_documento_veces_impreso IS 'Número de veces que se ha impreso el documento';

-- Tabla: prestamo_documento_impresion
-- Registra cada impresión de un documento (auditoría)
CREATE TABLE IF NOT EXISTS prestamo_documento_impresion (
    prestamo_documento_impresionid BIGSERIAL PRIMARY KEY,
    prestamo_documentoid BIGINT NOT NULL,
    prestamo_documento_impresion_fecha TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    prestamo_documento_impresion_usuario VARCHAR(40),
    prestamo_documento_impresion_ip VARCHAR(50),
    prestamo_documento_impresion_observaciones VARCHAR(500),

    CONSTRAINT fk_prestamo_documento_impresion_documento
        FOREIGN KEY (prestamo_documentoid)
        REFERENCES prestamo_documento(prestamo_documentoid)
        ON DELETE CASCADE
);

-- Índices para prestamo_documento_impresion
CREATE INDEX idx_prestamo_documento_impresion_documentoid ON prestamo_documento_impresion(prestamo_documentoid);
CREATE INDEX idx_prestamo_documento_impresion_fecha ON prestamo_documento_impresion(prestamo_documento_impresion_fecha);
CREATE INDEX idx_prestamo_documento_impresion_usuario ON prestamo_documento_impresion(prestamo_documento_impresion_usuario);

-- Comentarios
COMMENT ON TABLE prestamo_documento_impresion IS 'Registro de auditoría de impresiones de documentos de préstamos';
COMMENT ON COLUMN prestamo_documento_impresion.prestamo_documento_impresion_ip IS 'Dirección IP desde donde se realizó la impresión';

-- Insertar datos de ejemplo (opcional)
-- Este comentario muestra cómo se usarían las tablas, pero no se ejecuta automáticamente

/*
-- Ejemplo de uso:
-- Al generar un contrato por primera vez:
INSERT INTO prestamo_documento (prestamoid, prestamo_documento_tipo, prestamo_documento_user_crea)
VALUES (1, 'CONTRATO', 'admin');

-- Al imprimir el contrato:
INSERT INTO prestamo_documento_impresion (prestamo_documentoid, prestamo_documento_impresion_usuario, prestamo_documento_impresion_ip)
VALUES (1, 'admin', '192.168.1.100');

-- Actualizar contador de impresiones:
UPDATE prestamo_documento
SET prestamo_documento_veces_impreso = prestamo_documento_veces_impreso + 1,
    prestamo_documento_fecha_ultima_impresion = CURRENT_TIMESTAMP,
    prestamo_documento_fecha_primera_impresion = COALESCE(prestamo_documento_fecha_primera_impresion, CURRENT_TIMESTAMP)
WHERE prestamo_documentoid = 1;
*/

-- Vista para estadísticas de documentos por préstamo
CREATE OR REPLACE VIEW v_prestamo_documentos_estadisticas AS
SELECT
    p.prestamoid,
    p.prestamonumero,
    per.personanombrecompleto,
    pd.prestamo_documento_tipo,
    pd.prestamo_documento_veces_impreso,
    pd.prestamo_documento_fecha_primera_impresion,
    pd.prestamo_documento_fecha_ultima_impresion,
    COUNT(pdi.prestamo_documento_impresionid) as total_impresiones
FROM prestamo p
LEFT JOIN persona per ON p.personaid = per.personaid
LEFT JOIN prestamo_documento pd ON p.prestamoid = pd.prestamoid
LEFT JOIN prestamo_documento_impresion pdi ON pd.prestamo_documentoid = pdi.prestamo_documentoid
GROUP BY
    p.prestamoid,
    p.prestamonumero,
    per.personanombrecompleto,
    pd.prestamo_documento_tipo,
    pd.prestamo_documento_veces_impreso,
    pd.prestamo_documento_fecha_primera_impresion,
    pd.prestamo_documento_fecha_ultima_impresion;

COMMENT ON VIEW v_prestamo_documentos_estadisticas IS 'Vista de estadísticas de documentos de préstamos';

-- Función para registrar automáticamente una impresión
CREATE OR REPLACE FUNCTION fn_registrar_impresion_documento(
    p_prestamo_id BIGINT,
    p_tipo_documento VARCHAR(50),
    p_usuario VARCHAR(40),
    p_ip VARCHAR(50) DEFAULT NULL
) RETURNS BIGINT AS $$
DECLARE
    v_documento_id BIGINT;
    v_impresion_id BIGINT;
BEGIN
    -- Buscar o crear el documento
    SELECT prestamo_documentoid INTO v_documento_id
    FROM prestamo_documento
    WHERE prestamoid = p_prestamo_id
      AND prestamo_documento_tipo = p_tipo_documento;

    IF v_documento_id IS NULL THEN
        INSERT INTO prestamo_documento (
            prestamoid,
            prestamo_documento_tipo,
            prestamo_documento_user_crea,
            prestamo_documento_fecha_creacion
        )
        VALUES (
            p_prestamo_id,
            p_tipo_documento,
            p_usuario,
            CURRENT_TIMESTAMP
        )
        RETURNING prestamo_documentoid INTO v_documento_id;
    END IF;

    -- Registrar la impresión
    INSERT INTO prestamo_documento_impresion (
        prestamo_documentoid,
        prestamo_documento_impresion_fecha,
        prestamo_documento_impresion_usuario,
        prestamo_documento_impresion_ip
    )
    VALUES (
        v_documento_id,
        CURRENT_TIMESTAMP,
        p_usuario,
        p_ip
    )
    RETURNING prestamo_documento_impresionid INTO v_impresion_id;

    -- Actualizar contadores en el documento
    UPDATE prestamo_documento
    SET prestamo_documento_veces_impreso = prestamo_documento_veces_impreso + 1,
        prestamo_documento_fecha_ultima_impresion = CURRENT_TIMESTAMP,
        prestamo_documento_fecha_primera_impresion = COALESCE(prestamo_documento_fecha_primera_impresion, CURRENT_TIMESTAMP)
    WHERE prestamo_documentoid = v_documento_id;

    RETURN v_impresion_id;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION fn_registrar_impresion_documento IS 'Registra una impresión de documento de préstamo y actualiza contadores';

-- Permisos (ajustar según el usuario de la aplicación)
-- GRANT SELECT, INSERT, UPDATE ON prestamo_documento TO alphacredit_app;
-- GRANT SELECT, INSERT ON prestamo_documento_impresion TO alphacredit_app;
-- GRANT SELECT ON v_prestamo_documentos_estadisticas TO alphacredit_app;
-- GRANT EXECUTE ON FUNCTION fn_registrar_impresion_documento TO alphacredit_app;

PRINT 'Migración 005_prestamo_documentos.sql aplicada exitosamente';
