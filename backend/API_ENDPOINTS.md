# AlphaCredit API - Endpoints Disponibles

## Base URL
- Desarrollo: `http://localhost:5000`

---

## 1. Health Check

### GET /api/health
Verifica el estado del servicio.

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2025-10-15T18:00:00Z",
  "service": "AlphaCredit API"
}
```

---

## 2. Personas

### GET /api/personas
Lista todas las personas con paginación.

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `searchTerm` (optional) - Busca en nombre, identificación o email

**Response Headers:**
- `X-Total-Count` - Total de registros
- `X-Page-Number` - Página actual
- `X-Page-Size` - Tamaño de página

### GET /api/personas/{id}
Obtiene una persona por ID con datos relacionados.

### GET /api/personas/clientes
Obtiene solo los clientes activos.

### POST /api/personas
Crea una nueva persona.

**Request Body:**
```json
{
  "personaPrimerNombre": "Juan",
  "personaSegundoNombre": "Carlos",
  "personaPrimerApellido": "Pérez",
  "personaSegundoApellido": "López",
  "tipoIdentificacionId": 1,
  "personaIdentificacion": "0801199012345",
  "personaFechaNacimiento": "1990-01-15",
  "personaEsNatural": true,
  "personaEsCliente": true,
  "estadoCivilId": 1,
  "sexoId": 1,
  "personaDireccion": "Col. Kennedy, Tegucigalpa",
  "personaEmail": "juan.perez@example.com",
  "personaUsercrea": "admin"
}
```

### PUT /api/personas/{id}
Actualiza una persona existente.

### DELETE /api/personas/{id}
Elimina o desactiva una persona (soft delete si tiene préstamos).

---

## 3. Préstamos

### GET /api/prestamos
Lista todos los préstamos con paginación y filtros.

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `personaId` (optional) - Filtrar por persona
- `estadoId` (optional) - Filtrar por estado

### GET /api/prestamos/{id}
Obtiene un préstamo por ID con componentes y garantías.

### GET /api/prestamos/persona/{personaId}
Obtiene todos los préstamos de una persona.

### GET /api/prestamos/activos
Obtiene todos los préstamos activos con saldo pendiente.

### POST /api/prestamos
Crea un nuevo préstamo.

**Request Body:**
```json
{
  "personaId": 1,
  "sucursalId": 1,
  "monedaId": 1,
  "estadoPrestamoId": 1,
  "frecuenciaPagoId": 1,
  "destinoCreditoId": 1,
  "prestamoMonto": 50000.00,
  "prestamoTasaInteres": 18.50,
  "prestamoPlazo": 12,
  "prestamoFechaAprobacion": "2025-10-15",
  "prestamoFechaDesembolso": "2025-10-15",
  "prestamoFechaVencimiento": "2026-10-15",
  "prestamoUserCrea": "admin"
}
```

### PUT /api/prestamos/{id}
Actualiza un préstamo existente (campos permitidos).

### DELETE /api/prestamos/{id}
Elimina un préstamo (solo si no tiene pagos).

---

## 4. Garantías

### GET /api/garantias
Lista todas las garantías con paginación y filtros.

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `personaId` (optional)
- `tipoGarantiaId` (optional)
- `estadoGarantiaId` (optional)

### GET /api/garantias/{id}
Obtiene una garantía por ID.

### GET /api/garantias/persona/{personaId}
Obtiene todas las garantías de una persona.

### GET /api/garantias/disponibles
Obtiene garantías disponibles para asignar a préstamos.

### POST /api/garantias
Crea una nueva garantía.

### PUT /api/garantias/{id}
Actualiza una garantía existente.

### DELETE /api/garantias/{id}
Elimina una garantía (valida si está asociada a préstamos).

---

## 5. Fondos

### GET /api/fondos
Lista todos los fondos con paginación y filtros.

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `tipoFondoId` (optional)
- `monedaId` (optional)
- `soloActivos` (optional, default: false)

### GET /api/fondos/{id}
Obtiene un fondo por ID con movimientos.

### GET /api/fondos/activos
Obtiene todos los fondos activos.

### GET /api/fondos/disponibles
Obtiene fondos disponibles con saldo opcional.

**Query Parameters:**
- `saldoMinimo` (optional)

### GET /api/fondos/{id}/saldo
Obtiene información detallada del saldo del fondo.

**Response:**
```json
{
  "fondoId": 1,
  "fondoNombre": "Fondo Principal",
  "saldoInicial": 1000000.00,
  "saldoActual": 850000.00,
  "totalIngresos": 50000.00,
  "totalEgresos": 200000.00,
  "saldoCalculado": 850000.00
}
```

### POST /api/fondos
Crea un nuevo fondo.

**Request Body:**
```json
{
  "fondoNombre": "Fondo de Microcréditos",
  "fondoSaldoInicial": 1000000.00,
  "tipofondossbid": 1,
  "fondoActivo": true,
  "fondoFechaCrea": "2025-10-15",
  "fondoUserCrea": "admin"
}
```

### PUT /api/fondos/{id}
Actualiza un fondo existente.

### DELETE /api/fondos/{id}
Elimina o desactiva un fondo (soft delete si tiene movimientos).

---

## 6. Catálogos

Todos los catálogos tienen los mismos endpoints con el formato:
`/api/catalogs/{catalog-name}`

### Catálogos Disponibles:

1. **sexo** - Géneros
2. **estadocivil** - Estados civiles
3. **tipoidentificacion** - Tipos de identificación
4. **operadortelefono** - Operadoras telefónicas
5. **moneda** - Monedas
6. **frecuenciapago** - Frecuencias de pago
7. **formapago** - Formas de pago
8. **destinocredito** - Destinos del crédito
9. **estadoprestamo** - Estados de préstamo
10. **tipogarantia** - Tipos de garantía
11. **estadogarantia** - Estados de garantía
12. **componenteprestamo** - Componentes de préstamo
13. **estadocomponente** - Estados de componente
14. **tipocuenta** - Tipos de cuenta bancaria
15. **tipofondo** - Tipos de fondo

### GET /api/catalogs/{catalog-name}
Lista elementos del catálogo.

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `soloActivos` (optional, default: false)

**Ejemplo:**
```
GET /api/catalogs/sexo
GET /api/catalogs/estadocivil
GET /api/catalogs/moneda
```

### GET /api/catalogs/{catalog-name}/{id}
Obtiene un elemento del catálogo por ID.

### POST /api/catalogs/{catalog-name}
Crea un nuevo elemento del catálogo.

### PUT /api/catalogs/{catalog-name}/{id}
Actualiza un elemento del catálogo.

### DELETE /api/catalogs/{catalog-name}/{id}
Elimina un elemento del catálogo (valida si está en uso).

---

## Códigos de Estado HTTP

- `200 OK` - Solicitud exitosa
- `201 Created` - Recurso creado exitosamente
- `204 No Content` - Actualización/eliminación exitosa
- `400 Bad Request` - Datos inválidos o regla de negocio violada
- `404 Not Found` - Recurso no encontrado
- `500 Internal Server Error` - Error del servidor

---

## Ejemplos de Uso con cURL

### Listar personas
```bash
curl http://localhost:5000/api/personas?pageNumber=1&pageSize=10
```

### Crear persona
```bash
curl -X POST http://localhost:5000/api/personas \
  -H "Content-Type: application/json" \
  -d '{
    "personaPrimerNombre": "Juan",
    "personaPrimerApellido": "Pérez",
    "tipoIdentificacionId": 1,
    "personaIdentificacion": "0801199012345",
    "personaFechaNacimiento": "1990-01-15",
    "personaEsNatural": true,
    "personaEsCliente": true,
    "personaDireccion": "Tegucigalpa",
    "personaUsercrea": "admin"
  }'
```

### Obtener catálogo de monedas
```bash
curl http://localhost:5000/api/catalogs/moneda
```

### Crear préstamo
```bash
curl -X POST http://localhost:5000/api/prestamos \
  -H "Content-Type: application/json" \
  -d '{
    "personaId": 1,
    "sucursalId": 1,
    "monedaId": 1,
    "estadoPrestamoId": 1,
    "frecuenciaPagoId": 1,
    "prestamoMonto": 50000,
    "prestamoTasaInteres": 18.5,
    "prestamoPlazo": 12,
    "prestamoUserCrea": "admin"
  }'
```

---

## Notas

- Todos los endpoints GET list soportan paginación
- Los endpoints PUT y POST validan datos antes de guardar
- Los DELETE implementan soft delete cuando hay relaciones
- Las respuestas incluyen mensajes de error descriptivos
- Se incluyen las entidades relacionadas en las consultas GET by ID
