# 🔧 Solución Error 500 al Generar Documentos

## ❌ Problema

```
Error al generar el documento: Request failed with status code 500
```

## 🔍 Causa

El error 500 ocurre porque las tablas de documentos **NO EXISTEN** en la base de datos:
- `prestamo_documento`
- `prestamo_documento_impresion`

## ✅ Solución

### Paso 1: Verificar si las tablas existen

Ejecuta este query en PostgreSQL (pgAdmin, DBeaver, etc.):

```sql
-- Verificar tablas
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
AND table_name IN ('prestamo_documento', 'prestamo_documento_impresion');
```

**Si el resultado está vacío o solo muestra 1 tabla:** Las tablas NO existen.

---

### Paso 2: Ejecutar la Migración

#### Opción A: Desde pgAdmin o DBeaver

1. Abrir tu cliente SQL
2. Conectar a la base de datos `alphacredit`
3. Abrir el archivo: `C:\Proyectos\alphacredit\backend\migrations\005_prestamo_documentos.sql`
4. Ejecutar todo el script (F5 o botón Ejecutar)
5. Verificar que muestra: "Migración 005_prestamo_documentos.sql aplicada exitosamente"

#### Opción B: Copiar y Pegar Manualmente

Si tienes problemas con el archivo, copia y pega este script directamente:

```sql
-- ============================================
-- MIGRACIÓN: Tablas de Documentos de Préstamos
-- ============================================

-- Tabla: prestamo_documento
CREATE TABLE IF NOT EXISTS prestamo_documento (
    prestamo_documentoid BIGSERIAL PRIMARY KEY,
    prestamoid BIGINT NOT NULL,
    prestamo_documento_tipo VARCHAR(50) NOT NULL,
    prestamo_documento_ruta VARCHAR(500),
    prestamo_documento_hash VARCHAR(100),
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

-- Índices
CREATE INDEX IF NOT EXISTS idx_prestamo_documento_prestamoid ON prestamo_documento(prestamoid);
CREATE INDEX IF NOT EXISTS idx_prestamo_documento_tipo ON prestamo_documento(prestamo_documento_tipo);

-- Tabla: prestamo_documento_impresion
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

-- Índices
CREATE INDEX IF NOT EXISTS idx_prestamo_documento_impresion_documentoid ON prestamo_documento_impresion(prestamo_documentoid);
CREATE INDEX IF NOT EXISTS idx_prestamo_documento_impresion_fecha ON prestamo_documento_impresion(prestamo_documento_impresion_fecha);
CREATE INDEX IF NOT EXISTS idx_prestamo_documento_impresion_usuario ON prestamo_documento_impresion(prestamo_documento_impresion_usuario);

-- Vista de estadísticas
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

-- Función para registrar impresiones
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

    -- Actualizar contadores
    UPDATE prestamo_documento
    SET prestamo_documento_veces_impreso = prestamo_documento_veces_impreso + 1,
        prestamo_documento_fecha_ultima_impresion = CURRENT_TIMESTAMP,
        prestamo_documento_fecha_primera_impresion = COALESCE(prestamo_documento_fecha_primera_impresion, CURRENT_TIMESTAMP)
    WHERE prestamo_documentoid = v_documento_id;

    RETURN v_impresion_id;
END;
$$ LANGUAGE plpgsql;

-- Mensaje de éxito
DO $$
BEGIN
    RAISE NOTICE '✅ Migración completada exitosamente';
END $$;
```

---

### Paso 3: Verificar que se crearon las tablas

Ejecuta nuevamente:

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
AND table_name IN ('prestamo_documento', 'prestamo_documento_impresion');
```

**Deberías ver:**
```
prestamo_documento
prestamo_documento_impresion
```

---

### Paso 4: Reiniciar la API

```bash
# Si la API está corriendo, detenerla (Ctrl+C)
# Luego reiniciarla:
cd C:\Proyectos\alphacredit\backend\AlphaCredit.Api
dotnet run
```

---

### Paso 5: Probar Nuevamente

1. Ir al frontend: `http://localhost:3000`
2. Consultar un préstamo existente
3. Click en "📄 Ver Documentos"
4. Click en "🖨️ Generar e Imprimir" en cualquier documento

**Ahora debería funcionar correctamente** ✅

---

## 🔍 Verificación Adicional

Si aún falla, revisa los logs del backend:

```bash
# Los logs deberían mostrar:
info: AlphaCredit.Api.Controllers.PrestamosController[0]
      Generando documentos para préstamo {PrestamoId}
info: AlphaCredit.Api.Controllers.PrestamosController[0]
      Documentos generados exitosamente para préstamo {PrestamoId}
```

Si ves errores tipo:
```
error: Microsoft.EntityFrameworkCore
       Table 'prestamo_documento' does not exist
```

**Significa que la migración NO se ejecutó correctamente.**

---

## 📝 Notas Importantes

1. **La migración es OBLIGATORIA** - Sin las tablas, el sistema NO funciona
2. **Ejecutar UNA SOLA VEZ** - Las tablas solo deben crearse una vez
3. **Sin pérdida de datos** - Esta migración solo CREA tablas nuevas, no modifica nada existente
4. **Compatible con PostgreSQL 12+**

---

## 🆘 Si Persiste el Error

Verifica:

1. ✅ Las tablas existen: `\dt prestamo_documento*` en psql
2. ✅ La API está conectada a la BD correcta
3. ✅ El usuario de PostgreSQL tiene permisos de escritura
4. ✅ No hay errores de compilación en el backend

---

## 📞 Script de Diagnóstico Rápido

Ejecuta esto para un diagnóstico completo:

```sql
-- Diagnóstico completo
SELECT
    'Tabla prestamo_documento' as objeto,
    CASE WHEN COUNT(*) > 0 THEN '✅ Existe' ELSE '❌ No existe' END as estado
FROM information_schema.tables
WHERE table_name = 'prestamo_documento'
UNION ALL
SELECT
    'Tabla prestamo_documento_impresion' as objeto,
    CASE WHEN COUNT(*) > 0 THEN '✅ Existe' ELSE '❌ No existe' END as estado
FROM information_schema.tables
WHERE table_name = 'prestamo_documento_impresion'
UNION ALL
SELECT
    'Vista v_prestamo_documentos_estadisticas' as objeto,
    CASE WHEN COUNT(*) > 0 THEN '✅ Existe' ELSE '❌ No existe' END as estado
FROM information_schema.views
WHERE table_name = 'v_prestamo_documentos_estadisticas'
UNION ALL
SELECT
    'Función fn_registrar_impresion_documento' as objeto,
    CASE WHEN COUNT(*) > 0 THEN '✅ Existe' ELSE '❌ No existe' END as estado
FROM information_schema.routines
WHERE routine_name = 'fn_registrar_impresion_documento';
```

**Resultado esperado:** Todos con ✅ Existe

---

**Fecha:** 2025-10-19
**Versión:** 1.0.0
**Urgencia:** Alta - Requerido para funcionamiento del sistema
