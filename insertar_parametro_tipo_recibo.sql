-- Script para insertar parámetro de configuración del tipo de recibo
-- Valores posibles: 'A4' o 'POS'

-- Primero verificamos si ya existe el parámetro
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM parametrossistema
        WHERE parametrosistemallave = 'TIPO_RECIBO'
    ) THEN
        INSERT INTO parametrossistema (
            parametrosistemallave,
            parametrossistemavalor,
            parametrossistemadescripcion,
            parametrossistematipodato,
            parametrossistemaestaactivo
        ) VALUES (
            'TIPO_RECIBO',
            'POS',
            'Tipo de formato de recibo para impresión: A4 (tamaño carta) o POS (impresora térmica 80mm)',
            'STRING',
            true
        );
    ELSE
        -- Si ya existe, actualizamos la descripción pero mantenemos el valor actual
        UPDATE parametrossistema
        SET parametrossistemadescripcion = 'Tipo de formato de recibo para impresión: A4 (tamaño carta) o POS (impresora térmica 80mm)'
        WHERE parametrosistemallave = 'TIPO_RECIBO';
    END IF;
END $$;

-- Nota: Cambiar el valor a 'A4' si deseas usar formato carta
-- UPDATE parametrossistema SET parametrossistemavalor = 'A4' WHERE parametrosistemallave = 'TIPO_RECIBO';

-- O cambiar a 'POS' para impresoras térmicas
-- UPDATE parametrossistema SET parametrossistemavalor = 'POS' WHERE parametrosistemallave = 'TIPO_RECIBO';
