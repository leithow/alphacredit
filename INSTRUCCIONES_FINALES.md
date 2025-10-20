# ✅ Instrucciones Finales - Sistema de Documentos de Préstamos

## 🎉 Estado Actual

### ✅ Completado:

1. **Sistema de Documentos Implementado**
   - ✅ Contrato de préstamo basado en normativa hondureña
   - ✅ Pagaré con formato legal válido
   - ✅ Plan de pagos con tabla de amortización
   - ✅ Control de impresiones con auditoría completa

2. **Restricciones y Permisos**
   - ✅ Préstamos NO se pueden editar una vez creados
   - ✅ Préstamos SÍ se pueden consultar (modo solo lectura)
   - ✅ Frontend actualizado con modo vista

3. **Generación Automática**
   - ✅ Contrato y Pagaré se generan AUTOMÁTICAMENTE al crear préstamo
   - ✅ Registro de primera impresión automático
   - ✅ Plan de pagos disponible bajo demanda

4. **Parametrización**
   - ✅ Formato de moneda Lempiras (HNL) en todo el frontend
   - ✅ Configuración centralizada en `.env`

5. **Backend**
   - ✅ Compilación exitosa sin errores
   - ✅ Modelos corregidos (PersonaDocumento y GarantiaDocumento)
   - ✅ Servicios de generación de documentos listos

---

## 📋 Pasos Pendientes (Manual)

### 1️⃣ Ejecutar Migración SQL (REQUERIDO)

**Opción A: Desde pgAdmin o DBeaver**
1. Abrir pgAdmin o DBeaver
2. Conectar a la base de datos `alphacredit`
3. Abrir el archivo: `C:\Proyectos\alphacredit\backend\migrations\005_prestamo_documentos.sql`
4. Ejecutar el script completo
5. Verificar que se crearon las tablas:
   - `prestamo_documento`
   - `prestamo_documento_impresion`

**Opción B: Desde línea de comandos (si psql está instalado)**
```bash
psql -U postgres -d alphacredit -f "C:\Proyectos\alphacredit\backend\migrations\005_prestamo_documentos.sql"
```

**⚠️ IMPORTANTE:** Sin esta migración, la aplicación NO funcionará correctamente.

---

### 2️⃣ Verificar Funcionamiento

#### A) Iniciar la API

```bash
cd C:\Proyectos\alphacredit\backend\AlphaCredit.Api
dotnet run
```

La API debería iniciar en: `http://localhost:5000`

#### B) Iniciar el Frontend

```bash
cd C:\Proyectos\alphacredit\frontend\alphacredit-app
npm start
```

El frontend debería abrir en: `http://localhost:3000`

---

### 3️⃣ Probar el Sistema

#### Crear un Préstamo y Verificar Generación Automática

1. **Ir a la aplicación web**: http://localhost:3000
2. **Navegar a**: Préstamos → Nuevo Préstamo
3. **Llenar formulario**:
   - Seleccionar cliente
   - Ingresar monto, tasa, plazo
   - Seleccionar fondo
   - Hacer clic en "Crear Préstamo"

4. **Verificar en la consola del backend**:
   ```
   info: AlphaCredit.Api.Controllers.PrestamosController[0]
         Generando documentos para préstamo {PrestamoId}
   info: AlphaCredit.Api.Controllers.PrestamosController[0]
         Documentos generados exitosamente para préstamo {PrestamoId}
   ```

5. **Verificar en la base de datos**:
   ```sql
   -- Ver documentos generados
   SELECT * FROM prestamo_documento WHERE prestamoid = [ID_DEL_PRESTAMO];

   -- Ver impresiones registradas
   SELECT * FROM prestamo_documento_impresion;

   -- Vista de estadísticas
   SELECT * FROM v_prestamo_documentos_estadisticas;
   ```

#### Consultar un Préstamo (Modo Vista)

1. **Ir a lista de préstamos**
2. **Hacer clic en "Editar"** o navegar a `/prestamos/edit/{id}`
3. **Verificar**:
   - ✅ Se muestra mensaje "📋 Consultar Préstamo"
   - ✅ Banner amarillo: "Modo Solo Lectura"
   - ✅ Todos los campos están deshabilitados (grises)
   - ✅ Solo aparece botón "← Volver"
   - ✅ NO aparece botón "Guardar" o "Actualizar"

#### Acceder a Documentos del Préstamo

**Opción 1: Usando la API directamente**

```bash
# Contrato (reemplazar {id} con ID real)
curl http://localhost:5000/api/prestamos/{id}/documentos/contrato > contrato.html

# Pagaré
curl http://localhost:5000/api/prestamos/{id}/documentos/pagare > pagare.html

# Plan de Pagos
curl http://localhost:5000/api/prestamos/{id}/documentos/plan-pagos > plan.html

# Estadísticas
curl http://localhost:5000/api/prestamos/{id}/documentos/estadisticas
```

**Opción 2: Abrir en navegador**

- Contrato: `http://localhost:5000/api/prestamos/{id}/documentos/contrato`
- Pagaré: `http://localhost:5000/api/prestamos/{id}/documentos/pagare`
- Plan: `http://localhost:5000/api/prestamos/{id}/documentos/plan-pagos`

Los documentos se abrirán con formato HTML listo para imprimir.

---

## 🔧 Integración del Modal de Documentos (Opcional)

Si deseas agregar un botón "📋 Documentos" en la lista de préstamos para facilitar el acceso:

### Editar PrestamosList.js

```jsx
import React, { useState } from 'react';
import PrestamoDocumentosModal from '../../components/common/PrestamoDocumentosModal';

function PrestamosList() {
  // ... código existente ...

  // Agregar estados
  const [showDocsModal, setShowDocsModal] = useState(false);
  const [prestamoSeleccionado, setPrestamoSeleccionado] = useState(null);

  return (
    <>
      {/* ... tu tabla de préstamos ... */}

      {/* En cada fila, agregar botón */}
      <td className="actions">
        <button
          onClick={() => {
            setPrestamoSeleccionado(prestamo);
            setShowDocsModal(true);
          }}
          className="btn-docs"
        >
          📋 Documentos
        </button>
      </td>

      {/* Al final del componente */}
      {prestamoSeleccionado && (
        <PrestamoDocumentosModal
          isOpen={showDocsModal}
          onClose={() => {
            setShowDocsModal(false);
            setPrestamoSeleccionado(null);
          }}
          prestamo={prestamoSeleccionado}
        />
      )}
    </>
  );
}
```

---

## 📊 Verificación de Auditoría

### Consultar Historial de Impresiones

```sql
-- Ver todas las impresiones de un préstamo
SELECT
    pd.prestamo_documento_tipo,
    pd.prestamo_documento_veces_impreso,
    pdi.prestamo_documento_impresion_fecha,
    pdi.prestamo_documento_impresion_usuario,
    pdi.prestamo_documento_impresion_ip
FROM prestamo_documento pd
LEFT JOIN prestamo_documento_impresion pdi ON pd.prestamo_documentoid = pdi.prestamo_documentoid
WHERE pd.prestamoid = [ID_DEL_PRESTAMO]
ORDER BY pdi.prestamo_documento_impresion_fecha DESC;
```

### Usar la API

```bash
# Ver historial de contrato
curl "http://localhost:5000/api/prestamos/{id}/documentos/historial?tipoDocumento=CONTRATO"

# Ver estadísticas generales
curl http://localhost:5000/api/prestamos/{id}/documentos/estadisticas
```

---

## 📁 Archivos Importantes

### Backend
- ✅ `Models/PrestamoDocumento.cs` - Modelo de documentos
- ✅ `Models/PrestamoDocumentoImpresion.cs` - Modelo de impresiones
- ✅ `Services/PrestamoDocumentoService.cs` - Servicio de generación
- ✅ `Controllers/PrestamosController.cs` - Endpoints actualizados
- ⚠️ `migrations/005_prestamo_documentos.sql` - **PENDIENTE DE EJECUTAR**

### Frontend
- ✅ `pages/prestamos/PrestamoForm.js` - Modo vista implementado
- ✅ `components/common/PrestamoDocumentosModal.js` - Modal de documentos
- ✅ `services/prestamoDocumentoService.js` - Cliente API
- ✅ `utils/currency.js` - Formato HNL
- ✅ `.env` - Configuración de moneda

### Documentación
- ✅ `DOCUMENTOS_PRESTAMOS_README.md` - Documentación completa
- ✅ `INSTRUCCIONES_FINALES.md` - Este archivo

---

## 🎯 Flujo Completo de Uso

### 1. Usuario crea un préstamo
```
1. Usuario llena formulario
2. Click en "Crear Préstamo"
3. Backend crea préstamo
4. Backend genera automáticamente:
   ✓ Contrato de préstamo
   ✓ Pagaré
   ✓ Registra primera "impresión" de cada uno
5. Usuario recibe confirmación
```

### 2. Usuario consulta préstamo existente
```
1. Usuario va a lista de préstamos
2. Click en "Editar" o botón de consulta
3. Frontend abre en modo SOLO LECTURA
4. Usuario ve toda la información
5. Todos los campos están deshabilitados
6. Solo puede volver atrás
```

### 3. Usuario accede a documentos
```
Opción A: Vía API directa
- GET /api/prestamos/{id}/documentos/contrato
- GET /api/prestamos/{id}/documentos/pagare
- GET /api/prestamos/{id}/documentos/plan-pagos

Opción B: Vía Modal (si integrado)
- Click en "📋 Documentos"
- Modal muestra 3 documentos disponibles
- Click en "Generar e Imprimir"
- Se abre ventana nueva con documento
- Se registra nueva impresión
- Contador se incrementa
```

### 4. Auditoría
```
- Cada acceso a documento se registra
- Se guarda: fecha, hora, usuario, IP
- Contador de impresiones se actualiza
- Historial disponible vía API o BD
```

---

## ⚠️ Importante

### NO Funciona Sin Migración
El sistema **requiere** que ejecutes la migración SQL. Sin las tablas:
- ❌ Creación de préstamos fallará
- ❌ No se podrán generar documentos
- ❌ Error al iniciar la aplicación

### Orden de Ejecución
1. ✅ Migración SQL (PRIMERO)
2. ✅ Iniciar Backend
3. ✅ Iniciar Frontend
4. ✅ Probar creación de préstamo

---

## 🆘 Troubleshooting

### Error: "Cannot use table 'personadocumento'"
**Solución:** Ya está corregido en el código. Solo reiniciar la API.

### Error: "prestamo_documento table does not exist"
**Solución:** Ejecutar la migración SQL.

### Documentos no se generan automáticamente
**Verificar:**
1. Migración SQL ejecutada ✓
2. Backend sin errores en consola ✓
3. Logs muestran "Generando documentos..." ✓

### Frontend muestra pantalla de error
**Solución:** Los cambios en PrestamoForm.js ya están aplicados. Refrescar navegador (Ctrl+F5).

---

## 📞 Siguiente Paso

**👉 EJECUTAR MIGRACIÓN SQL AHORA**

```sql
-- Abrir este archivo en tu cliente SQL:
C:\Proyectos\alphacredit\backend\migrations\005_prestamo_documentos.sql

-- O ejecutar desde terminal:
psql -U postgres -d alphacredit -f "backend/migrations/005_prestamo_documentos.sql"
```

Una vez ejecutada la migración, el sistema estará **100% funcional** y listo para usar! 🚀

---

**Fecha:** 2025-10-19
**Versión:** 1.0.0
**Estado:** ✅ Backend Listo | ✅ Frontend Listo | ⚠️ Migración Pendiente
