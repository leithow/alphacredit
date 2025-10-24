-- Crear tabla pagodetalle para registrar detalles de pagos aplicados a componentes

CREATE TABLE IF NOT EXISTS pagodetalle (
    pagodetalleid BIGSERIAL PRIMARY KEY,
    movimientoprestamoid BIGINT NOT NULL,
    prestamocomponenteid BIGINT,
    componenteprestamoid BIGINT NOT NULL,
    pagodetallecuotanumero INTEGER,
    pagodetallemontoaplicado DECIMAL(18,2) NOT NULL DEFAULT 0,
    pagodetallemontoantes DECIMAL(18,2) NOT NULL DEFAULT 0,
    pagodetallefechaaplicacion DATE NOT NULL,

    -- Claves foráneas
    CONSTRAINT fk_pagodetalle_movimientoprestamo
        FOREIGN KEY (movimientoprestamoid)
        REFERENCES movimientoprestamo(movimientoprestamoid)
        ON DELETE CASCADE,

    CONSTRAINT fk_pagodetalle_prestamocomponente
        FOREIGN KEY (prestamocomponenteid)
        REFERENCES prestamocomponente(prestamocomponenteid)
        ON DELETE SET NULL,

    CONSTRAINT fk_pagodetalle_componenteprestamo
        FOREIGN KEY (componenteprestamoid)
        REFERENCES componenteprestamo(componenteprestamoid)
        ON DELETE RESTRICT
);

-- Índices para mejorar el rendimiento
CREATE INDEX IF NOT EXISTS idx_pagodetalle_movimientoprestamo
    ON pagodetalle(movimientoprestamoid);

CREATE INDEX IF NOT EXISTS idx_pagodetalle_prestamocomponente
    ON pagodetalle(prestamocomponenteid);

CREATE INDEX IF NOT EXISTS idx_pagodetalle_componenteprestamo
    ON pagodetalle(componenteprestamoid);

-- Comentarios
COMMENT ON TABLE pagodetalle IS 'Detalle de pagos aplicados a componentes específicos de cuotas de préstamos';
COMMENT ON COLUMN pagodetalle.pagodetallemontoaplicado IS 'Monto que se aplicó al componente en este pago';
COMMENT ON COLUMN pagodetalle.pagodetallemontoantes IS 'Saldo del componente antes de aplicar este pago';
COMMENT ON COLUMN pagodetalle.pagodetallefechaaplicacion IS 'Fecha en que se aplicó el pago';
