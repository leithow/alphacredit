# Sistema de Documentos de Préstamos - AlphaCredit

## Descripción General

Se ha implementado un sistema completo de generación de documentos para préstamos, incluyendo:

1. **Contrato de Préstamo** - Documento legal basado en normativa hondureña
2. **Pagaré** - Documento de compromiso de pago
3. **Plan de Pagos** - Tabla de amortización detallada
4. **Control de Impresiones** - Auditoría de todas las impresiones de documentos

## Características Principales

### 🔒 Restricción de Edición de Préstamos

- **Los préstamos NO pueden editarse una vez creados**
- Esta restricción aplica tanto en el frontend como en el backend
- Razón: Integridad financiera y auditoría
- El endpoint PUT `/api/prestamos/{id}` retorna error 400 con mensaje explicativo
- En el frontend, si se intenta editar un préstamo, se muestra una pantalla informativa

### 📄 Documentos Generados

#### 1. Contrato de Préstamo
- Documento HTML formateado para impresión
- Incluye todas las cláusulas legales basadas en normativa hondureña:
  - Objeto del contrato
  - Intereses y tasas
  - Plazo y forma de pago
  - Cláusulas de mora
  - Garantías (si existen)
  - Legislación aplicable
- Conversión automática de números a letras
- Espacios para firmas del acreedor y deudor

#### 2. Pagaré
- Formato legal válido en Honduras
- Incluye monto total a pagar (capital + intereses)
- Fecha de vencimiento
- Cláusulas de mora e incumplimiento
- Espacio para firma del deudor

#### 3. Plan de Pagos
- Tabla completa de amortización
- Desglose por cuota: capital, interés, cuota total, saldo
- Información del préstamo y cliente
- Resumen de totales
- Diseño profesional para impresión

### 📊 Control de Impresiones

El sistema registra automáticamente:
- **Fecha y hora** de cada impresión
- **Usuario** que generó el documento
- **Dirección IP** desde donde se imprimió
- **Contador de impresiones** por documento
- **Primera y última impresión** de cada documento

### 💰 Parametrización de Moneda

#### Configuración (`.env`)
```env
REACT_APP_CURRENCY_LOCALE=es-HN
REACT_APP_CURRENCY_CODE=HNL
REACT_APP_CURRENCY_SYMBOL=L
```

#### Utilidad de Formateo
Se ha creado una utilidad centralizada en `/src/utils/currency.js`:

```javascript
import { formatCurrency } from '../../utils/currency';

// Uso:
formatCurrency(1000)  // Retorna: "L 1,000.00"
formatCurrency(1500.50, 0, 0)  // Sin decimales: "L 1,501"
```

**Todos los archivos del frontend ahora usan esta utilidad**, asegurando consistencia en el formato de moneda Lempiras (HNL).

## Base de Datos

### Nuevas Tablas

#### `prestamo_documento`
Almacena los documentos generados:
- `prestamo_documentoid` (PK)
- `prestamoid` (FK)
- `prestamo_documento_tipo` (CONTRATO, PAGARE, PLAN_PAGOS)
- `prestamo_documento_veces_impreso`
- `prestamo_documento_fecha_primera_impresion`
- `prestamo_documento_fecha_ultima_impresion`
- Auditoría: user_crea, fecha_creacion

#### `prestamo_documento_impresion`
Registro detallado de cada impresión:
- `prestamo_documento_impresionid` (PK)
- `prestamo_documentoid` (FK)
- `prestamo_documento_impresion_fecha`
- `prestamo_documento_impresion_usuario`
- `prestamo_documento_impresion_ip`
- `prestamo_documento_impresion_observaciones`

### Vistas y Funciones

#### Vista: `v_prestamo_documentos_estadisticas`
Muestra estadísticas completas de documentos por préstamo.

#### Función: `fn_registrar_impresion_documento`
Registra automáticamente una impresión y actualiza contadores.

```sql
SELECT fn_registrar_impresion_documento(
    p_prestamo_id := 1,
    p_tipo_documento := 'CONTRATO',
    p_usuario := 'admin',
    p_ip := '192.168.1.100'
);
```

### Migración SQL

La migración se encuentra en:
```
backend/migrations/005_prestamo_documentos.sql
```

**Ejecutar con:**
```bash
psql -U postgres -d alphacredit -f backend/migrations/005_prestamo_documentos.sql
```

## API Endpoints

### Generación de Documentos

#### `GET /api/prestamos/{id}/documentos/contrato`
Genera el contrato de préstamo.

**Query Params:**
- `usuario` (opcional): Usuario que genera el documento

**Response:** HTML del contrato

**Ejemplo:**
```bash
GET /api/prestamos/1/documentos/contrato?usuario=admin
```

#### `GET /api/prestamos/{id}/documentos/pagare`
Genera el pagaré del préstamo.

**Query Params:**
- `usuario` (opcional): Usuario que genera el documento

**Response:** HTML del pagaré

#### `GET /api/prestamos/{id}/documentos/plan-pagos`
Genera el plan de pagos del préstamo.

**Query Params:**
- `usuario` (opcional): Usuario que genera el documento

**Response:** HTML del plan de pagos

### Auditoría y Estadísticas

#### `GET /api/prestamos/{id}/documentos/historial`
Obtiene el historial de impresiones de un documento.

**Query Params:**
- `tipoDocumento` (requerido): CONTRATO, PAGARE o PLAN_PAGOS

**Response:**
```json
{
  "prestamoId": 1,
  "tipoDocumento": "CONTRATO",
  "totalImpresiones": 3,
  "impresiones": [
    {
      "fecha": "2025-10-19T10:30:00Z",
      "usuario": "admin",
      "ip": "192.168.1.100",
      "observaciones": null
    }
  ]
}
```

#### `GET /api/prestamos/{id}/documentos/estadisticas`
Obtiene estadísticas de todos los documentos de un préstamo.

**Response:**
```json
{
  "prestamoId": 1,
  "documentos": [
    {
      "tipo": "CONTRATO",
      "vecesImpreso": 3,
      "primeraImpresion": "2025-10-19T10:00:00Z",
      "ultimaImpresion": "2025-10-19T15:30:00Z"
    },
    {
      "tipo": "PAGARE",
      "vecesImpreso": 2,
      "primeraImpresion": "2025-10-19T10:05:00Z",
      "ultimaImpresion": "2025-10-19T14:00:00Z"
    }
  ]
}
```

#### `PUT /api/prestamos/{id}` (DESHABILITADO)
Retorna error 400.

**Response:**
```json
{
  "message": "Los préstamos no pueden ser modificados una vez creados por razones de integridad financiera y auditoría.",
  "code": "PRESTAMO_NO_EDITABLE"
}
```

## Frontend

### Componente: PrestamoDocumentosModal

Componente React para gestionar documentos de préstamos.

**Ubicación:** `frontend/alphacredit-app/src/components/common/PrestamoDocumentosModal.js`

**Props:**
- `isOpen` (boolean): Controla visibilidad del modal
- `onClose` (function): Callback para cerrar el modal
- `prestamo` (object): Objeto del préstamo con sus datos

**Características:**
- Visualización de estadísticas de documentos
- Botones para generar e imprimir cada documento
- Historial detallado de impresiones
- Interfaz intuitiva con iconos y badges

**Uso:**
```jsx
import PrestamoDocumentosModal from '../../components/common/PrestamoDocumentosModal';

function PrestamosList() {
  const [showDocs, setShowDocs] = useState(false);
  const [prestamoSeleccionado, setPrestamoSeleccionado] = useState(null);

  return (
    <>
      <button onClick={() => {
        setPrestamoSeleccionado(prestamo);
        setShowDocs(true);
      }}>
        Ver Documentos
      </button>

      <PrestamoDocumentosModal
        isOpen={showDocs}
        onClose={() => setShowDocs(false)}
        prestamo={prestamoSeleccionado}
      />
    </>
  );
}
```

### Servicio: prestamoDocumentoService

**Ubicación:** `frontend/alphacredit-app/src/services/prestamoDocumentoService.js`

**Métodos:**

```javascript
// Generar documentos
await prestamoDocumentoService.generarContrato(prestamoId, usuario);
await prestamoDocumentoService.generarPagare(prestamoId, usuario);
await prestamoDocumentoService.generarPlanPagos(prestamoId, usuario);

// Auditoría
await prestamoDocumentoService.obtenerHistorialImpresiones(prestamoId, 'CONTRATO');
await prestamoDocumentoService.obtenerEstadisticas(prestamoId);

// Imprimir (abre ventana nueva)
prestamoDocumentoService.imprimirDocumento(htmlContent, 'Contrato');
```

### Restricción de Edición en Frontend

El componente `PrestamoForm` detecta automáticamente si está en modo edición:

```jsx
// Si isEditMode = true (URL: /prestamos/edit/:id)
// Muestra pantalla bloqueada con mensaje:
"⚠️ Préstamo No Editable"
"Por razones de integridad financiera y auditoría,
los préstamos no pueden modificarse una vez creados."
```

## Backend

### Servicio: PrestamoDocumentoService

**Ubicación:** `backend/AlphaCredit.Api/Services/PrestamoDocumentoService.cs`

**Métodos principales:**

```csharp
// Generación de documentos
Task<string> GenerarContratoPrestamo(long prestamoId)
Task<string> GenerarPagare(long prestamoId)
Task<string> GenerarPlanPagos(long prestamoId)

// Control de impresiones
Task<PrestamoDocumento> RegistrarImpresion(long prestamoId, string tipoDocumento, string usuario, string? ip)
Task<List<PrestamoDocumentoImpresion>> ObtenerHistorialImpresiones(long prestamoId, string tipoDocumento)
```

**Características técnicas:**
- Conversión de números a letras (español)
- Generación de HTML con estilos inline
- Cálculo automático de totales
- Integración con servicio de amortización
- Hash de documentos para verificar integridad (futuro)

### Modelos

#### PrestamoDocumento
```csharp
public class PrestamoDocumento
{
    public long PrestamoDocumentoId { get; set; }
    public long PrestamoId { get; set; }
    public string PrestamoDocumentoTipo { get; set; }
    public int PrestamoDocumentoVecesImpreso { get; set; }
    public DateTime? PrestamoDocumentoFechaPrimeraImpresion { get; set; }
    public DateTime? PrestamoDocumentoFechaUltimaImpresion { get; set; }
    // ... navegación y auditoría
}
```

#### PrestamoDocumentoImpresion
```csharp
public class PrestamoDocumentoImpresion
{
    public long PrestamoDocumentoImpresionId { get; set; }
    public long PrestamoDocumentoId { get; set; }
    public DateTime PrestamoDocumentoImpresionFecha { get; set; }
    public string? PrestamoDocumentoImpresionUsuario { get; set; }
    public string? PrestamoDocumentoImpresionIp { get; set; }
    // ... navegación
}
```

## Flujo de Uso

### 1. Crear un Préstamo
```
1. Usuario crea préstamo en /prestamos/new
2. Sistema genera automáticamente:
   - Número de préstamo
   - Tabla de amortización
   - Componentes (capital e interés)
   - Movimiento de fondo (desembolso)
3. Préstamo queda BLOQUEADO para edición
```

### 2. Generar Documentos
```
1. Usuario accede a lista de préstamos
2. Click en "Ver Documentos" del préstamo deseado
3. Se abre modal con 3 documentos disponibles:
   - Contrato de Préstamo
   - Pagaré
   - Plan de Pagos
4. Usuario selecciona documento y click en "Generar e Imprimir"
5. Sistema:
   - Genera HTML del documento
   - Registra impresión en BD
   - Abre ventana nueva con documento
   - Ejecuta comando de impresión del navegador
6. Estadísticas se actualizan automáticamente
```

### 3. Consultar Historial
```
1. En modal de documentos, click en "Ver Historial"
2. Se muestra tabla con:
   - Fecha y hora de cada impresión
   - Usuario que imprimió
   - IP de origen
3. Click en "Ocultar Historial" para cerrar
```

## Normativa Legal (Honduras)

Los documentos generados incluyen cláusulas basadas en:

- **Código de Comercio de Honduras**
- **Código Civil de Honduras**
- Regulaciones de la **Comisión Nacional de Bancos y Seguros (CNBS)**

### Cláusulas Principales

1. **Del Objeto**: Monto, entrega y obligación de devolución
2. **De los Intereses**: Tasa anual y cálculo
3. **Del Plazo y Forma de Pago**: Cuotas y fechas
4. **De la Mora**: Intereses moratorios y vencimiento anticipado
5. **De las Garantías**: Listado de garantías constituidas
6. **Del Domicilio**: Domicilio del deudor
7. **De la Legislación Aplicable**: Leyes hondureñas

## Seguridad y Auditoría

### Registro Automático
- Cada generación/impresión se registra automáticamente
- No es posible generar documentos sin registro
- IP del cliente se captura automáticamente

### Integridad de Datos
- Préstamos inmutables después de creación
- Hash de documentos (preparado para futura implementación)
- Índices únicos en BD para evitar duplicados

### Trazabilidad
- Fecha/hora de primera y última impresión
- Contador de veces impreso
- Usuario responsable de cada impresión
- Historial completo de acciones

## Testing

### Backend

```bash
# Probar generación de contrato
curl http://localhost:5000/api/prestamos/1/documentos/contrato?usuario=test

# Probar estadísticas
curl http://localhost:5000/api/prestamos/1/documentos/estadisticas

# Probar historial
curl "http://localhost:5000/api/prestamos/1/documentos/historial?tipoDocumento=CONTRATO"
```

### Frontend

1. Crear un préstamo de prueba
2. Ir a lista de préstamos
3. Click en "Ver Documentos"
4. Probar generación de cada documento
5. Verificar que se abra ventana de impresión
6. Revisar estadísticas actualizadas
7. Ver historial de impresiones

## Próximas Mejoras

### Funcionalidades Pendientes
- [ ] Exportación a PDF (usando biblioteca como jsPDF o iText)
- [ ] Firma digital de documentos
- [ ] Envío de documentos por email
- [ ] Plantillas personalizables por sucursal
- [ ] Almacenamiento de PDFs generados en sistema de archivos
- [ ] Verificación de integridad con hash SHA-256
- [ ] Documentos adicionales (comprobantes de pago, cartas de liberación)
- [ ] Marca de agua en documentos

### Mejoras Técnicas
- [ ] Caché de documentos generados
- [ ] Compresión de HTML
- [ ] Mejora en conversión de números a letras (decimales)
- [ ] Soporte multi-idioma
- [ ] Pruebas unitarias e integración
- [ ] Documentación OpenAPI/Swagger

## Troubleshooting

### El documento no se imprime
- Verificar que las ventanas emergentes estén habilitadas
- Revisar configuración de impresora predeterminada
- Comprobar que JavaScript esté habilitado

### Error "PRESTAMO_NO_EDITABLE"
- Es comportamiento esperado
- Los préstamos no se pueden editar una vez creados
- Contactar administrador si requiere cambios

### Estadísticas no se actualizan
- Refrescar el modal cerrando y abriendo nuevamente
- Verificar conexión a la API
- Revisar consola del navegador para errores

### Formato de moneda incorrecto
- Verificar archivo `.env` tenga configuración correcta:
  ```
  REACT_APP_CURRENCY_CODE=HNL
  REACT_APP_CURRENCY_LOCALE=es-HN
  ```
- Reiniciar servidor de desarrollo después de cambiar `.env`

## Soporte

Para preguntas o problemas:
- Revisar logs del backend: `backend/logs/`
- Revisar consola del navegador (F12)
- Contactar al equipo de desarrollo

---

**Versión:** 1.0.0
**Fecha:** Octubre 2025
**Autor:** Sistema AlphaCredit
**Licencia:** Propietaria
