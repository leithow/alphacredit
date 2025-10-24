# Cambios Implementados - Sistema de Recibos y Configuración de Moneda

**Fecha:** 23 de Octubre, 2025
**Rama:** `leithow`

## Resumen

Se implementó un sistema completo de generación de recibos con soporte dual para formatos A4 e impresoras POS térmicas (80mm), junto con la configuración centralizada de moneda del sistema.

---

## 1. Configuración de Moneda Centralizada

### Archivos Creados

#### `backend/AlphaCredit.Api/Models/CurrencySettings.cs`
Modelo de configuración para la moneda del sistema.

```csharp
public class CurrencySettings
{
    public string Code { get; set; } = "HNL";      // Código ISO
    public string Symbol { get; set; } = "L";       // Símbolo
    public string Name { get; set; } = "LEMPIRAS";  // Nombre
    public string Locale { get; set; } = "es-HN";   // Locale
}
```

### Archivos Modificados

#### `backend/AlphaCredit.Api/appsettings.json`
Se agregó la sección de configuración de moneda:

```json
"Currency": {
  "Code": "HNL",
  "Symbol": "L",
  "Name": "LEMPIRAS",
  "Locale": "es-HN"
}
```

#### `backend/AlphaCredit.Api/Program.cs`
Se registró el servicio de configuración de moneda (líneas 31-32):

```csharp
builder.Services.Configure<CurrencySettings>(
    builder.Configuration.GetSection("Currency"));
```

### Beneficios
- Moneda configurable desde un solo punto
- Fácil adaptación para otros países/monedas
- Consistencia en todo el sistema
- No más valores hardcodeados

---

## 2. Sistema Dual de Recibos (A4 / POS)

### Archivos Creados

#### `insertar_parametro_tipo_recibo.sql`
Script SQL para crear el parámetro de configuración del tipo de recibo.

**Características:**
- Verifica si el parámetro ya existe antes de insertar
- Valor por defecto: `'POS'`
- Valores válidos: `'A4'` o `'POS'`
- Si ya existe, solo actualiza la descripción

**Uso:**
```sql
-- Cambiar a formato A4
UPDATE parametrossistema
SET parametrossistemavalor = 'A4'
WHERE parametrosistemallave = 'TIPO_RECIBO';

-- Cambiar a formato POS
UPDATE parametrossistema
SET parametrossistemavalor = 'POS'
WHERE parametrosistemallave = 'TIPO_RECIBO';
```

### Archivos Modificados

#### `backend/AlphaCredit.Api/Services/ReciboAbonoService.cs`

**Cambios principales:**

1. **Inyección de Configuración de Moneda**
   ```csharp
   private readonly CurrencySettings _currencySettings;

   public ReciboAbonoService(
       AlphaCreditDbContext context,
       IOptions<CurrencySettings> currencySettings)
   {
       _context = context;
       _currencySettings = currencySettings.Value;
   }
   ```

2. **Método para Formatear Moneda**
   ```csharp
   private string FormatearMoneda(decimal monto)
   {
       return $"{_currencySettings.Symbol} {monto:N2}";
   }
   ```
   - Reemplazó todos los `"C$ {monto:N2}"` hardcodeados
   - Usa el símbolo configurado en `appsettings.json`

3. **Método para Obtener Tipo de Recibo**
   ```csharp
   private async Task<string> ObtenerTipoReciboAsync()
   {
       var parametro = await _context.Set<ParametrosSistema>()
           .FirstOrDefaultAsync(p => p.ParametroSistemaLlave == "TIPO_RECIBO");

       return parametro?.ParametrosSistemaValor ?? "A4";
   }
   ```
   - Lee el parámetro de la base de datos
   - Valor por defecto: "A4" si no existe

4. **Generación Condicional de Recibos**

   Método `GenerarReciboPdfAsync` actualizado para soportar ambos formatos:

   ```csharp
   var tipoRecibo = await ObtenerTipoReciboAsync();

   if (tipoRecibo == "POS")
   {
       // Generar recibo formato POS
       documento = Document.Create(container => {
           container.Page(page => {
               page.Size(226, 10000); // 80mm ancho, altura dinámica
               page.Margin(5);
               page.DefaultTextStyle(x => x.FontSize(8));
               page.Content().Element(c => CrearReciboPOS(c, datos));
           });
       });
   }
   else
   {
       // Generar recibo formato A4 (existente)
       documento = Document.Create(container => {
           container.Page(page => {
               page.Size(PageSizes.Letter);
               page.Margin(40);
               page.DefaultTextStyle(x => x.FontSize(10));
               page.Header().Element(c => CrearEncabezado(c, datos));
               page.Content().Element(c => CrearContenido(c, datos));
               page.Footer().Element(c => CrearPiePagina(c, datos));
           });
       });
   }
   ```

5. **Nuevo Método: CrearReciboPOS**

   Método completo (157 líneas) para generar recibos optimizados para impresoras térmicas.

---

## 3. Diseño del Recibo POS (80mm)

### Especificaciones Técnicas
- **Ancho:** 80mm (~226 puntos)
- **Alto:** Dinámico según contenido
- **Márgenes:** 5 puntos
- **Fuente base:** 8pt

### Estructura del Recibo

```
┌─────────────────────────────────┐
│     ENCABEZADO EMPRESA          │
│     (Nombre, Sucursal)          │
│     Dirección, Teléfono         │
├─────────────────────────────────┤
│      RECIBO DE PAGO             │
│         No. 12345               │
├─────────────────────────────────┤
│ Fecha: 23/10/2025  Hora: 14:30 │
│ Préstamo No: 100                │
│ Cliente: Juan Pérez García      │
├─────────────────────────────────┤
│ DETALLE DEL PAGO                │
│ Capital:           L 1,250.00   │
│ Interés:             L 150.00   │
│ Mora:                 L 25.00   │
│ Otros Cargos:          L 0.00   │
├─────────────────────────────────┤
│ TOTAL:             L 1,425.00   │
│ SON: MIL CUATROCIENTOS          │
│      VEINTICINCO LEMPIRAS       │
├─────────────────────────────────┤
│ Forma de Pago: EFECTIVO         │
├─────────────────────────────────┤
│ SALDOS RESTANTES                │
│ Capital Pendiente: L 8,750.00   │
│ Interés Pendiente:   L 850.00   │
│ Mora Pendiente:       L 75.00   │
├─────────────────────────────────┤
│ Observaciones:                  │
│ Pago correspondiente a cuota... │
├─────────────────────────────────┤
│ Firma del Cliente:              │
│ _____________________________   │
├─────────────────────────────────┤
│ Generado: 23/10/2025 14:30:25  │
└─────────────────────────────────┘
```

### Características del Diseño

1. **Encabezado** (10pt, negrita, centrado)
   - Nombre de la empresa
   - Nombre de la sucursal
   - Dirección y teléfono (6-7pt)

2. **Título del Recibo** (9pt, negrita, centrado)
   - "RECIBO DE PAGO"
   - Número de recibo (7pt)

3. **Información General** (7pt)
   - Fecha y hora del pago
   - Número de préstamo
   - Nombre completo del cliente

4. **Detalle del Pago** (7pt)
   - Título "DETALLE DEL PAGO" (8pt, negrita)
   - Desglose por componente (Capital, Interés, Mora, Otros)
   - Alineación: etiquetas a la izquierda, montos a la derecha

5. **Total** (9pt, negrita)
   - Monto total en números
   - Monto en letras (6pt)
   - Usa la moneda configurada

6. **Forma de Pago** (7pt)
   - Método utilizado (EFECTIVO, TRANSFERENCIA, etc.)

7. **Saldos Restantes** (7pt)
   - Capital pendiente
   - Interés pendiente
   - Mora pendiente

8. **Observaciones** (6pt)
   - Campo opcional para notas adicionales

9. **Firma del Cliente** (7pt)
   - Espacio para firma física
   - Línea de separación

10. **Footer** (5pt, centrado, gris)
    - Timestamp de generación del recibo

### Optimizaciones para Impresoras Térmicas

- **Fuentes reducidas** (5-10pt) para aprovechar espacio
- **Layout vertical** en una sola columna
- **Líneas horizontales** para separar secciones
- **Texto centrado** en encabezados y títulos
- **Alto dinámico** se ajusta al contenido
- **Márgenes mínimos** (5pt) para maximizar área imprimible

---

## 4. Actualización de Referencias de Moneda

### Cambios Globales en ReciboAbonoService.cs

Todas las referencias hardcodeadas a moneda fueron reemplazadas:

**Antes:**
```csharp
.Text($"C$ {datos.MontoCapital:N2}")
.Text("CÓRDOBAS")
```

**Después:**
```csharp
.Text(FormatearMoneda(datos.MontoCapital))
.Text(_currencySettings.Name)
```

### Ubicaciones Actualizadas

1. **Método CrearEncabezado** (Formato A4)
   - Total del recibo
   - Monto en letras

2. **Método CrearContenido** (Formato A4)
   - Desglose de componentes
   - Saldos restantes

3. **Método CrearReciboPOS** (Formato POS)
   - Todo el recibo usa `FormatearMoneda()`
   - Nombre de moneda en monto en letras

---

## 5. Flujo de Generación de Recibos

```
┌─────────────────────────────────┐
│   Usuario realiza un abono      │
│   desde el frontend              │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│ PrestamosAbonosController       │
│ POST /api/prestamos-abonos      │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│ ReciboAbonoService              │
│ GenerarReciboPdfAsync()         │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│ ObtenerTipoReciboAsync()        │
│ Lee parametrossistema           │
│ Parámetro: TIPO_RECIBO          │
└──────────┬──────────────────────┘
           │
           ├─── POS ────┐
           │            ▼
           │   ┌─────────────────┐
           │   │ CrearReciboPOS  │
           │   │ (80mm, vertical)│
           │   └─────────────────┘
           │
           └─── A4 ────┐
                       ▼
              ┌─────────────────┐
              │ CrearEncabezado │
              │ CrearContenido  │
              │ CrearPiePagina  │
              │ (Letter, layout)│
              └─────────────────┘
                       │
                       ▼
              ┌─────────────────┐
              │ Genera PDF      │
              │ (QuestPDF)      │
              └─────────────────┘
                       │
                       ▼
              ┌─────────────────┐
              │ Retorna byte[]  │
              │ al frontend     │
              └─────────────────┘
                       │
                       ▼
              ┌─────────────────┐
              │ Frontend        │
              │ descarga PDF    │
              └─────────────────┘
```

---

## 6. Configuración de la Base de Datos

### Tabla: parametrossistema

Nueva entrada creada por el script SQL:

| Campo                          | Valor                                                                                          |
|--------------------------------|------------------------------------------------------------------------------------------------|
| parametrosistemallave          | TIPO_RECIBO                                                                                    |
| parametrossistemavalor         | POS                                                                                            |
| parametrossistemadescripcion   | Tipo de formato de recibo para impresión: A4 (tamaño carta) o POS (impresora térmica 80mm)   |
| parametrossistematipodato      | STRING                                                                                         |
| parametrossistemaestaactivo    | true                                                                                           |

---

## 7. Instrucciones de Uso

### Para Desarrolladores

1. **Cambiar tipo de recibo:**
   ```sql
   -- A formato A4
   UPDATE parametrossistema
   SET parametrossistemavalor = 'A4'
   WHERE parametrosistemallave = 'TIPO_RECIBO';

   -- A formato POS
   UPDATE parametrossistema
   SET parametrossistemavalor = 'POS'
   WHERE parametrosistemallave = 'TIPO_RECIBO';
   ```

2. **Cambiar moneda del sistema:**

   Editar `backend/AlphaCredit.Api/appsettings.json`:
   ```json
   "Currency": {
     "Code": "USD",
     "Symbol": "$",
     "Name": "DÓLARES",
     "Locale": "en-US"
   }
   ```

3. **Reiniciar backend** para aplicar cambios de configuración:
   ```bash
   cd backend/AlphaCredit.Api
   dotnet run
   ```

### Para Usuarios Finales

1. Acceder al sistema de préstamos
2. Seleccionar un préstamo
3. Realizar un abono
4. El sistema automáticamente:
   - Procesa el pago
   - Genera el recibo según configuración
   - Descarga el PDF

---

## 8. Archivos Afectados

### Nuevos Archivos
- `backend/AlphaCredit.Api/Models/CurrencySettings.cs`
- `insertar_parametro_tipo_recibo.sql`
- `CAMBIOS_SISTEMA_RECIBOS.md` (este archivo)

### Archivos Modificados
- `backend/AlphaCredit.Api/appsettings.json`
- `backend/AlphaCredit.Api/Program.cs`
- `backend/AlphaCredit.Api/Services/ReciboAbonoService.cs`

---

## 9. Dependencias

- **QuestPDF:** Librería de generación de PDFs (ya existente)
- **Entity Framework Core:** ORM para acceso a datos (ya existente)
- **PostgreSQL:** Base de datos (ya existente)

No se requieren nuevas dependencias.

---

## 10. Testing

### Casos de Prueba Recomendados

1. **Recibo POS con moneda configurada**
   - Configurar TIPO_RECIBO = 'POS'
   - Realizar abono
   - Verificar formato 80mm
   - Verificar símbolo "L" en montos
   - Verificar "LEMPIRAS" en monto en letras

2. **Recibo A4 con moneda configurada**
   - Configurar TIPO_RECIBO = 'A4'
   - Realizar abono
   - Verificar formato carta
   - Verificar símbolo "L" en montos
   - Verificar "LEMPIRAS" en monto en letras

3. **Cambio dinámico de formato**
   - Generar recibo en POS
   - Cambiar parámetro a A4
   - Generar nuevo recibo
   - Verificar diferencia de formatos

4. **Recibo con todos los componentes**
   - Abono que incluya: Capital, Interés, Mora, Otros
   - Verificar suma correcta
   - Verificar monto en letras correcto
   - Verificar saldos restantes

5. **Recibo con observaciones**
   - Abono con campo de observaciones
   - Verificar que se muestre correctamente
   - Verificar salto de línea si es largo

---

## 11. Notas Técnicas

### QuestPDF - Lecciones Aprendidas

1. **PageSize:**
   - Usar `page.Size(width, height)` con valores numéricos
   - No usar `new PageSize(width, Unit.Point)`

2. **FontSize en Rows:**
   - `Row()` retorna `void`, no se puede encadenar `.FontSize()`
   - Aplicar `.FontSize()` a cada elemento de texto individual:
     ```csharp
     row.RelativeItem().Text("Texto").FontSize(7);
     ```

3. **Layout Dinámico:**
   - Para altura dinámica, usar valor grande (ej: 10000)
   - El PDF final se ajusta al contenido real

4. **Márgenes en POS:**
   - Márgenes mínimos (5pt) para maximizar espacio
   - Importante para impresoras con área imprimible limitada

### PostgreSQL - Manejo de Conflictos

- No usar `ON CONFLICT` si no hay constraint único/primary key
- Preferir bloques `DO` con verificación `IF EXISTS`
- Permite mayor control sobre insert/update

---

## 12. Próximos Pasos Sugeridos

1. **Pruebas de impresión física**
   - Imprimir en impresora térmica real
   - Verificar márgenes y corte
   - Ajustar tamaños de fuente si es necesario

2. **Configuración de impresora por sucursal**
   - Permitir que cada sucursal tenga su tipo de recibo
   - Agregar campo `sucursaltiporecibo` en tabla sucursales

3. **Logo de empresa en recibo**
   - Agregar soporte para imagen en encabezado
   - Configurar ruta de logo en `appsettings.json`

4. **Plantillas de recibo personalizables**
   - Sistema de templates para diferentes tipos de negocio
   - Permitir personalización de campos mostrados

5. **Envío de recibo por email**
   - Opción para enviar PDF adjunto al correo del cliente
   - Integración con servicio de email

6. **Historial de recibos**
   - Almacenar PDFs generados en sistema de archivos
   - Permitir reimpresión de recibos anteriores

---

## 13. Soporte y Contacto

Para preguntas o problemas relacionados con estos cambios:

- **Desarrollador:** [Tu Nombre]
- **Rama:** `leithow`
- **Fecha de implementación:** 23 de Octubre, 2025

---

## 14. Changelog

### [23/10/2025] - Implementación Inicial

**Agregado:**
- Sistema dual de recibos (A4 / POS)
- Configuración centralizada de moneda
- Modelo `CurrencySettings`
- Método `CrearReciboPOS` para recibos térmicos
- Script SQL para parámetro TIPO_RECIBO
- Método `FormatearMoneda` helper

**Modificado:**
- `ReciboAbonoService.cs` - Soporte para ambos formatos
- `appsettings.json` - Sección Currency
- `Program.cs` - Registro de CurrencySettings

**Corregido:**
- Referencias hardcodeadas a "C$" y "CÓRDOBAS"
- Formato de recibo ahora configurable desde BD

---

**Fin del Documento**
