# Sistema de Abonos a Préstamos - AlphaCredit

## Resumen

Se ha implementado un sistema completo de gestión de abonos a préstamos que soporta múltiples tipos de pago, integración automática con fondos y cálculo de mora.

## Características Implementadas

### 1. Tipos de Abonos Soportados

#### a) Abono a Cuota Completa (`CUOTA`)
- Paga cuotas completas en orden de vencimiento
- Prioriza cuotas vencidas primero
- Distribuye automáticamente entre capital e interés
- Si sobra dinero, continúa con la siguiente cuota

#### b) Abono Parcial (`PARCIAL`)
- Permite pagar parte de una cuota
- Distribución automática: Mora → Interés → Capital
- Permite distribución manual especificando montos exactos
- Puede aplicarse a una cuota específica o general

#### c) Abono a Capital (`CAPITAL`)
- Abono extraordinario directo al capital
- Reduce el saldo principal sin afectar intereses
- Útil para reducir el monto total del préstamo

#### d) Abono a Mora (`MORA`)
- Pago específico para saldar mora acumulada
- Se aplica directamente al saldo de mora del préstamo

### 2. Integración con Fondos

- Cada forma de pago puede estar asociada a un fondo específico
- Al realizar un abono, automáticamente se registra un movimiento de **INGRESO** en el fondo correspondiente
- El saldo del fondo se actualiza automáticamente
- Ejemplo: Pago de L 1,000 por transferencia bancaria → se registra ingreso en el fondo asociado a "Transferencia Bancaria"

### 3. Cálculo de Mora

- Cálculo automático basado en días vencidos
- Tasa de mora configurable en parámetros del sistema
- Aplica mora solo a componentes vencidos con saldo pendiente
- Fórmula: `Mora = Saldo * Tasa Mora Diaria * Días Vencidos`

### 4. Estado de Cuenta

- Vista completa del préstamo con detalle de cuotas
- Muestra capital, interés y mora de cada cuota
- Resumen de cuotas pagadas, pendientes y vencidas
- Historial completo de pagos realizados

## Estructura de Archivos Creados

### Backend - Models
- `PagoDetalle.cs` - Detalle de pagos aplicados a componentes

### Backend - DTOs
- `AbonoPrestamoRequest.cs` - Request para aplicar abonos
- `AbonoPrestamoResponse.cs` - Response con resultado del abono
- `EstadoCuentaPrestamoDto.cs` - Estado de cuenta completo
- `DistribucionAbonoDto.cs` - Distribución manual de abonos

### Backend - Services
- `PrestamoAbonoService.cs` - Lógica principal de aplicación de abonos
- `PrestamoMoraService.cs` - Cálculo de mora
- `PrestamoEstadoCuentaService.cs` - Generación de estados de cuenta

### Backend - Controllers
- `PrestamosAbonosController.cs` - API endpoints para abonos

### Backend - Migrations
- `migracion_abonos_prestamos.sql` - Script SQL completo

## API Endpoints

### POST `/api/prestamos/{prestamoId}/abonos`
Aplica un abono a un préstamo.

**Request Body:**
```json
{
  "prestamoId": 123,
  "monto": 1000.00,
  "formaPagoId": 1,
  "tipoAbono": "CUOTA",
  "numeroCuota": null,
  "distribucion": null,
  "observaciones": "Pago mensual",
  "fechaPago": "2025-10-22T10:30:00"
}
```

**Tipos de Abono:**
- `CUOTA` - Paga cuotas completas
- `PARCIAL` - Abono parcial
- `CAPITAL` - Abono a capital
- `MORA` - Pago de mora

**Response:**
```json
{
  "movimientoPrestamoId": 456,
  "montoAplicado": 1000.00,
  "montoCapital": 750.00,
  "montoInteres": 250.00,
  "montoMora": 0.00,
  "montoOtros": 0.00,
  "fechaAplicacion": "2025-10-22T10:30:00",
  "componentesAfectados": [
    {
      "prestamoComponenteId": 789,
      "componenteNombre": "Capital",
      "numeroCuota": 1,
      "montoAntes": 750.00,
      "montoAplicado": 750.00,
      "saldoNuevo": 0.00,
      "estadoNuevo": "Pagado"
    }
  ],
  "saldosNuevos": {
    "saldoCapital": 15000.00,
    "saldoInteres": 1500.00,
    "saldoMora": 0.00,
    "saldoTotal": 16500.00,
    "cuotasPendientes": 11,
    "cuotasPagadas": 1
  },
  "mensaje": "Abono aplicado a 1 cuota(s) completa(s)"
}
```

### GET `/api/prestamos/{prestamoId}/estado-cuenta`
Obtiene el estado de cuenta completo del préstamo.

**Response:**
```json
{
  "prestamoId": 123,
  "prestamoNumero": "PRE-2025-001",
  "nombreCliente": "Juan Pérez",
  "montoOriginal": 20000.00,
  "saldoCapital": 15000.00,
  "saldoInteres": 1500.00,
  "saldoMora": 150.00,
  "saldoTotal": 16650.00,
  "cuotas": [
    {
      "numeroCuota": 1,
      "fechaVencimiento": "2025-11-01",
      "diasVencidos": 0,
      "estaVencida": false,
      "estado": "Pendiente",
      "capitalMonto": 750.00,
      "capitalSaldo": 750.00,
      "interesMonto": 250.00,
      "interesSaldo": 250.00,
      "moraCalculada": 0.00,
      "cuotaTotal": 1000.00,
      "saldoTotal": 1000.00
    }
  ],
  "resumen": {
    "totalCuotas": 12,
    "cuotasPagadas": 0,
    "cuotasPendientes": 12,
    "cuotasVencidas": 0,
    "montoTotalPagado": 0.00,
    "montoTotalPendiente": 16650.00,
    "proximoPago": 1000.00,
    "fechaProximoPago": "2025-11-01"
  }
}
```

### GET `/api/prestamos/{prestamoId}/historial-pagos`
Obtiene el historial de todos los pagos realizados.

### GET `/api/prestamos/{prestamoId}/calcular-mora?fecha=2025-10-22`
Calcula la mora del préstamo a una fecha específica.

## Flujo de Operación

### 1. Aplicar Abono a Cuota Completa
```
1. Usuario selecciona préstamo y monto a pagar
2. Sistema calcula mora actualizada
3. Sistema identifica cuotas pendientes (vencidas primero)
4. Sistema aplica pago en orden: Mora → Interés → Capital
5. Si sobra dinero, pasa a la siguiente cuota
6. Actualiza saldos de componentes y préstamo
7. Crea movimiento en préstamo
8. Registra ingreso en fondo (según forma de pago)
9. Guarda detalles de pago por componente
```

### 2. Aplicar Abono Parcial con Distribución Manual
```
1. Usuario especifica distribución:
   - Mora: 50
   - Interés: 200
   - Capital: 750
2. Sistema aplica cada monto al tipo correspondiente
3. Actualiza saldos y estados
4. Registra movimiento y actualiza fondo
```

### 3. Integración con Fondos
```
Abono de L 1,000 con "Transferencia Bancaria":
├── Movimiento Préstamo: PAGO (L 1,000)
├── Actualiza saldos del préstamo
└── Movimiento Fondo: INGRESO (L 1,000)
    └── Actualiza saldo del fondo bancario
```

## Modelo de Datos

### Tabla: `pagodetalle`
Registra el detalle de cada componente afectado por un pago.

| Campo                        | Tipo          | Descripción                          |
|------------------------------|---------------|--------------------------------------|
| pagodetalleid                | BIGINT        | ID único                             |
| movimientoprestamoid         | BIGINT        | Movimiento de préstamo               |
| prestamocomponenteid         | BIGINT        | Componente afectado                  |
| componenteprestamoid         | BIGINT        | Tipo de componente                   |
| pagodetallecuotanumero       | INTEGER       | Número de cuota                      |
| pagodetallemontoaplicado     | DECIMAL(18,2) | Monto aplicado                       |
| pagodetallemontoantes        | DECIMAL(18,2) | Saldo antes del pago                 |
| pagodetallefechaaplicacion   | TIMESTAMP     | Fecha del pago                       |

### Modificación: `formapago`
Se agregó relación con fondo:

| Campo    | Tipo   | Descripción                      |
|----------|--------|----------------------------------|
| fondoid  | BIGINT | Fondo destino de los pagos       |

## Configuración del Sistema

### 1. Ejecutar Migración SQL
```bash
psql -U usuario -d alphacredit -f backend/AlphaCredit.Api/Migrations/migracion_abonos_prestamos.sql
```

### 2. Configurar Formas de Pago
Asignar fondos a cada forma de pago:

```sql
-- Ejemplo: Transferencia bancaria va al fondo "Banco BAC"
UPDATE formapago
SET fondoid = (SELECT fondoid FROM fondo WHERE fondonombre = 'Banco BAC')
WHERE formapaganombre = 'Transferencia Bancaria';

-- Efectivo va al fondo "Caja General"
UPDATE formapago
SET fondoid = (SELECT fondoid FROM fondo WHERE fondonombre = 'Caja General')
WHERE formapaganombre = 'Efectivo';
```

### 3. Configurar Tasa de Mora
```sql
-- Establecer tasa de mora anual (ejemplo: 24%)
UPDATE parametrossistema
SET parametrossistemavalor = '24.0'
WHERE parametrossistemanombre = 'TASA_MORA_ANUAL';
```

## Próximos Pasos (Frontend)

Para completar la implementación, se necesita crear las siguientes interfaces React:

1. **Pantalla de Abonos**
   - Formulario para registrar abonos
   - Selector de tipo de abono
   - Campos para distribución manual (opcional)
   - Vista previa del impacto del pago

2. **Estado de Cuenta**
   - Tabla de cuotas con estado
   - Resumen de saldos
   - Indicadores visuales de mora
   - Botón para imprimir

3. **Historial de Pagos**
   - Lista de pagos realizados
   - Filtros por fecha y tipo
   - Detalles de cada pago
   - Opción de reversión (admin)

## Notas Técnicas

- Todas las operaciones de abono se ejecutan dentro de una transacción para garantizar consistencia
- El cálculo de mora se actualiza automáticamente antes de cada abono
- Los componentes se actualizan en orden: Mora → Interés → Capital
- Los fondos se actualizan solo si la forma de pago tiene un fondo asociado
- El sistema valida que los montos aplicados no excedan los saldos pendientes

## Soporte y Mantenimiento

### Funciones Útiles en PostgreSQL

```sql
-- Calcular mora de un préstamo
SELECT fn_calcular_mora_prestamo(123, CURRENT_TIMESTAMP);

-- Ver estado de cuenta resumido
SELECT * FROM v_estado_cuenta_prestamos WHERE prestamoid = 123;

-- Ver detalles de un pago específico
SELECT pd.*, cp.componenteprestamonombre, pd.pagodetallemontoaplicado
FROM pagodetalle pd
JOIN componenteprestamo cp ON pd.componenteprestamoid = cp.componenteprestamoid
WHERE movimientoprestamoid = 456;
```

---

**Fecha de Implementación:** Octubre 2025
**Desarrollado para:** AlphaCredit - Sistema de Gestión de Microcréditos
