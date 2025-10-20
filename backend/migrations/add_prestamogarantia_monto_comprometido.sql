-- Agregar campo PrestamoGarantiaMontoComprometido a la tabla prestamogarantia
-- Fecha: 2025-10-18

-- Agregar columna si no existe
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_name = 'prestamogarantia'
        AND column_name = 'prestamogarantiamontocomprometido'
    ) THEN
        ALTER TABLE prestamogarantia
        ADD COLUMN prestamogarantiamontocomprometido DECIMAL(18,2) NOT NULL DEFAULT 0;

        RAISE NOTICE 'Columna prestamogarantiamontocomprometido agregada exitosamente';
    ELSE
        RAISE NOTICE 'La columna prestamogarantiamontocomprometido ya existe';
    END IF;
END $$;

-- Comentario descriptivo
COMMENT ON COLUMN prestamogarantia.prestamogarantiamontocomprometido IS 'Monto comprometido de la garantía para este préstamo específico';
