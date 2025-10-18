-- Agregar columna FondoId a la tabla Prestamo
ALTER TABLE prestamo
ADD COLUMN fondoid BIGINT NULL;

-- Agregar llave foránea a Fondo
ALTER TABLE prestamo
ADD CONSTRAINT FK_prestamo_fondo_fondoid
FOREIGN KEY (fondoid) REFERENCES fondo(fondoid);

-- Crear índice para mejorar el rendimiento de consultas
CREATE INDEX IDX_prestamo_fondoid ON prestamo(fondoid);

COMMENT ON COLUMN prestamo.fondoid IS 'ID del fondo del cual se desembolsó el préstamo';
