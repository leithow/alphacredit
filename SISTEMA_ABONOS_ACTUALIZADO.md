# Sistema de Abonos a Préstamos - Versión Actualizada con Mora como Componentes

## Cambios Importantes vs. Versión Anterior

### 1. **Mora como Componentes Separados**
- Anteriormente: La mora se calculaba como un monto único en `prestamo.prestamosaldomora`
- **Ahora**: La mora se registra como componentes individuales de tipo `MORA` en `prestamocomponente`
- **Ventaja**: Trazabilidad completa de cada cargo por mora, permite pagos selectivos

### 2. **Uso de Fecha del Sistema**
- Anteriormente: Se usaba `DateTime.Now` del servidor
- **Ahora**: Se usa la fecha de la tabla `fechasistema` (campo `fechasistemaestaactiva`)
- **Ventaja**: Control total de la fecha operativa del sistema

### 3. **Cálculo Diario Automatizado**
- Un job de PostgreSQL genera componentes de mora **diariamente**
- Solo genera mora del día actual (no recalcula días anteriores)
- Puede ejecutarse automáticamente con `pg_cron` o manualmente desde la API

## Arquitectura del Sistema de Mora

### Flujo Diario de Cálculo de Mora

```
1. Job Diario (00:00 o manual)
   ├── Obtiene fecha del sistema (fechasistema.fechasistemafecha)
   ├── Busca préstamos con cuotas vencidas
   │
   └── Para cada cuota vencida:
       ├── Calcula: mora_diaria = saldo * tasa_mora_diaria
       ├── Crea componente MORA:
       │   ├── Tipo: MORA
       │   ├── Monto: mora calculada del día
       │   ├── Fecha vencimiento: fecha actual
       │   ├── Número cuota: mismo que la cuota vencida
       │   └── Saldo: igual al monto
       │
       └── Actualiza prestamo.prestamosaldomora = SUM(componentes MORA)
```

### Ejemplo Práctico

**Préstamo:** L 10,000 a 12 meses
**Cuota #1:** L 1,000 (L 750 capital + L 250 interés)
**Fecha vencimiento:** 2025-01-15
**Tasa mora anual:** 24% (0.0657% diario)

#### Día 2025-01-16 (1 día vencido)
```sql
-- Saldo pendiente cuota #1: L 1,000
-- Mora diaria: 1,000 * 0.000657 = L 0.66

INSERT INTO prestamocomponente (
    prestamoid, componenteprestamoid, estadocomponenteid,
    prestamocomponentemonto, prestamocomponentesaldo,
    prestamocomponentefechavencimiento, prestamocomponentenumerocuota
) VALUES (
    123, [ID_MORA], [ID_PENDIENTE],
    0.66, 0.66,
    '2025-01-16', 1
);
```

#### Día 2025-01-17 (2 días vencido)
```sql
-- Se crea OTRO componente de mora para el día 17
-- Saldo pendiente cuota #1: L 1,000
-- Mora diaria: 1,000 * 0.000657 = L 0.66

INSERT INTO prestamocomponente (
    ...
    prestamocomponentefechavencimiento: '2025-01-17',
    ...
);
```

**Total mora acumulada:** L 1.32 (2 componentes de L 0.66 c/u)

## Orden de Aplicación de Pagos

**CRÍTICO**: El sistema SIEMPRE paga en este orden:

1. **MORA** (componentes tipo MORA, más antiguos primero)
2. **INTERÉS** (componentes tipo INTERES)
3. **CAPITAL** (componentes tipo CAPITAL)

### Ejemplo de Abono

**Estado del préstamo:**
- Cuota #1 vencida: L 750 capital + L 250 interés
- Mora acumulada (5 componentes): L 3.30

**Cliente paga L 500:**

```
Distribución automática:
├── Mora:     L 3.30 (paga los 5 componentes completos)
├── Interés:  L 250.00 (paga interés completo)
└── Capital:  L 246.70 (pago parcial a capital)
```

**Saldos después del pago:**
- Mora: L 0.00 ✅
- Interés cuota #1: L 0.00 ✅
- Capital cuota #1: L 503.30 (pendiente)

## Archivos Creados/Modificados

### Nuevos Servicios
1. **`FechaSistemaService.cs`** - Obtiene fecha operativa del sistema
2. **`PrestamoMoraCalculoService.cs`** - Genera componentes de mora diariamente

### Servicios Modificados
1. **`PrestamoAbonoService.cs`**
   - Usa `FechaSistemaService` en lugar de `DateTime.Now`
   - Paga componentes MORA primero
   - Actualiza `prestamosaldomora` desde componentes

### Controllers
1. **`PrestamosMoraController.cs`** - Endpoints para ejecutar cálculo de mora

### Scripts SQL
1. **`migracion_abonos_prestamos.sql`** - Migración actualizada
2. **`job_calculo_mora_diario.sql`** - Job de PostgreSQL para cálculo diario

## API Endpoints Actualizados

### POST `/api/prestamos/mora/calcular-diario`
Ejecuta el cálculo diario de mora manualmente.

**Response:**
```json
{
  "exitoso": true,
  "prestamosAfectados": 15,
  "duracionMs": 234,
  "mensaje": "Cálculo de mora completado. 15 préstamo(s) procesado(s).",
  "fechaEjecucion": "2025-10-22T00:00:15"
}
```

**Uso recomendado:**
- Llamar este endpoint diariamente usando un scheduler (.NET Hosted Service, Windows Task Scheduler, etc.)
- O usar pg_cron para ejecutar automáticamente en PostgreSQL

### GET `/api/prestamos/{id}/mora-total`
Obtiene la mora total actual de un préstamo.

**Response:**
```json
{
  "prestamoId": 123,
  "moraTotal": 15.50,
  "mensaje": "Mora total: L15.50"
}
```

## Configuración e Instalación

### 1. Ejecutar Migraciones

```bash
# Migración principal (crea tabla pagodetalle, componente MORA, etc.)
psql -U usuario -d alphacredit -f backend/AlphaCredit.Api/Migrations/migracion_abonos_prestamos.sql

# Job de cálculo diario de mora
psql -U usuario -d alphacredit -f backend/AlphaCredit.Api/Migrations/job_calculo_mora_diario.sql
```

### 2. Configurar Formas de Pago con Fondos

```sql
UPDATE formapago
SET fondoid = (SELECT fondoid FROM fondo WHERE fondonombre = 'Caja General')
WHERE formapaganombre = 'Efectivo';

UPDATE formapago
SET fondoid = (SELECT fondoid FROM fondo WHERE fondonombre = 'Banco BAC')
WHERE formapaganombre = 'Transferencia Bancaria';
```

### 3. Verificar Componente MORA

```sql
SELECT * FROM componenteprestamo WHERE componenteprestamotipo = 'MORA';
-- Debe existir un registro con este tipo
```

### 4. Configurar Fecha del Sistema

```sql
-- Insertar fecha operativa del sistema
INSERT INTO fechasistema (fechasistemafecha, fechasistemaestaactiva, fechasistemausercrea, fechasistemafechacreacion)
VALUES (CURRENT_DATE, true, 'ADMIN', CURRENT_TIMESTAMP);

-- Solo debe haber UNA fecha activa a la vez
UPDATE fechasistema SET fechasistemaestaactiva = false WHERE fechasistemaid != [ID_ACTUAL];
```

### 5. Programar Job Diario

**Opción A: Usar pg_cron (PostgreSQL)**
```sql
-- Requiere extensión instalada y permisos de superusuario
CREATE EXTENSION IF NOT EXISTS pg_cron;

SELECT cron.schedule(
    'calculo-mora-diario',
    '0 0 * * *',  -- Todos los días a las 00:00
    $$SELECT fn_ejecutar_calculo_mora_con_log()$$
);
```

**Opción B: Usar endpoint API desde .NET**

Crear un Hosted Service en tu aplicación:

```csharp
// Services/MoraCalculoHostedService.cs
public class MoraCalculoHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var ahora = DateTime.Now;
            var proximaEjecucion = ahora.Date.AddDays(1).AddHours(0); // Mañana a las 00:00

            await Task.Delay(proximaEjecucion - ahora, stoppingToken);

            using var scope = _serviceProvider.CreateScope();
            var moraService = scope.ServiceProvider.GetRequiredService<PrestamoMoraCalculoService>();

            await moraService.GenerarComponentesMoraDiariaAsync();
        }
    }
}

// Program.cs
builder.Services.AddHostedService<MoraCalculoHostedService>();
```

**Opción C: Usar Windows Task Scheduler**
```bash
# Crear tarea que ejecute curl diariamente
curl -X POST https://tudominio.com/api/prestamos/mora/calcular-diario
```

## Queries Útiles

### Ver componentes de mora de un préstamo
```sql
SELECT
    pc.prestamocomponentenumerocuota AS cuota,
    pc.prestamocomponentefechavencimiento AS fecha_mora,
    pc.prestamocomponentemonto AS mora_dia,
    pc.prestamocomponentesaldo AS saldo,
    ec.estadocomponentenombre AS estado
FROM prestamocomponente pc
INNER JOIN componenteprestamo cp ON pc.componenteprestamoid = cp.componenteprestamoid
INNER JOIN estadocomponente ec ON pc.estadocomponenteid = ec.estadocomponenteid
WHERE pc.prestamoid = 123
  AND cp.componenteprestamotipo = 'MORA'
ORDER BY pc.prestamocomponentefechavencimiento;
```

### Ver total de mora por préstamo
```sql
SELECT
    p.prestamoid,
    p.prestamonumero,
    COUNT(*) AS componentes_mora,
    SUM(pc.prestamocomponentesaldo) AS mora_total,
    p.prestamosaldomora AS mora_registrada
FROM prestamo p
INNER JOIN prestamocomponente pc ON p.prestamoid = pc.prestamoid
INNER JOIN componenteprestamo cp ON pc.componenteprestamoid = cp.componenteprestamoid
WHERE cp.componenteprestamotipo = 'MORA'
  AND pc.prestamocomponentesaldo > 0
GROUP BY p.prestamoid, p.prestamonumero, p.prestamosaldomora;
```

### Ver log de ejecuciones del job
```sql
SELECT
    logfechaejecucion,
    logprestamosafectados,
    logcomponentesgenerados,
    logduracionms,
    logmensaje
FROM log_calculo_mora
ORDER BY logfechaejecucion DESC
LIMIT 10;
```

## Diferencias Clave vs. Versión Anterior

| Aspecto | Versión Anterior | Versión Actual |
|---------|------------------|----------------|
| **Mora** | Saldo único en `prestamo.prestamosaldomora` | Componentes individuales tipo MORA |
| **Fecha** | `DateTime.Now` del servidor | `fechasistema.fechasistemafecha` |
| **Cálculo** | Al momento del abono | Job diario automático |
| **Trazabilidad** | Solo total acumulado | Cada día registrado por separado |
| **Pagos** | Distribuía entre capital/interés/mora | Paga componentes MORA primero siempre |

## Ventajas del Nuevo Sistema

✅ **Trazabilidad completa**: Cada día de mora es un registro separado
✅ **Control de fecha**: Usa fecha operativa del sistema
✅ **Auditoría**: Log de cada ejecución del job
✅ **Flexibilidad**: Permite reversar o ajustar moras específicas
✅ **Consistencia**: Siempre paga mora primero (orden garantizado)
✅ **Integración con fondos**: Automática al recibir pagos

## Consideraciones

⚠️ **Importante:** La tabla `fechasistema` debe tener **solo UNA fecha activa** a la vez
⚠️ El job debe ejecutarse **una vez al día** para evitar duplicados
⚠️ Si no tienes `pg_cron`, debes ejecutar el cálculo desde la API
⚠️ Los componentes MORA se generan solo para cuotas ya vencidas

---

**Fecha de Actualización:** Octubre 2025
**Versión:** 2.0 - Sistema con Mora como Componentes
