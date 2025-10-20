-- Script para verificar que las tablas de documentos existen
-- Ejecutar en PostgreSQL

-- Verificar tabla prestamo_documento
SELECT
    CASE
        WHEN EXISTS (
            SELECT FROM information_schema.tables
            WHERE table_schema = 'public'
            AND table_name = 'prestamo_documento'
        )
        THEN '✅ Tabla prestamo_documento EXISTE'
        ELSE '❌ Tabla prestamo_documento NO EXISTE - Ejecutar migración 005'
    END AS prestamo_documento_status;

-- Verificar tabla prestamo_documento_impresion
SELECT
    CASE
        WHEN EXISTS (
            SELECT FROM information_schema.tables
            WHERE table_schema = 'public'
            AND table_name = 'prestamo_documento_impresion'
        )
        THEN '✅ Tabla prestamo_documento_impresion EXISTE'
        ELSE '❌ Tabla prestamo_documento_impresion NO EXISTE - Ejecutar migración 005'
    END AS prestamo_documento_impresion_status;

-- Ver estructura de prestamo_documento si existe
SELECT column_name, data_type, character_maximum_length
FROM information_schema.columns
WHERE table_schema = 'public'
AND table_name = 'prestamo_documento'
ORDER BY ordinal_position;

-- Ver estructura de prestamo_documento_impresion si existe
SELECT column_name, data_type, character_maximum_length
FROM information_schema.columns
WHERE table_schema = 'public'
AND table_name = 'prestamo_documento_impresion'
ORDER BY ordinal_position;
