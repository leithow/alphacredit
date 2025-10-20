-- Agregar campo PrestamoGarantiaMontoComprometido a la tabla prestamogarantia
ALTER TABLE prestamogarantia ADD COLUMN IF NOT EXISTS prestamogarantiamontocomprometido DECIMAL(18,2) NOT NULL DEFAULT 0;
COMMENT ON COLUMN prestamogarantia.prestamogarantiamontocomprometido IS 'Monto comprometido de la garantía para este préstamo específico';
