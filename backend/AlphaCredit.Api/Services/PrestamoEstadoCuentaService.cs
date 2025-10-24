using AlphaCredit.Api.Data;
using AlphaCredit.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AlphaCredit.Api.Services;

/// <summary>
/// Servicio para generar estado de cuenta de préstamos
/// </summary>
public class PrestamoEstadoCuentaService
{
    private readonly AlphaCreditDbContext _context;
    private readonly PrestamoMoraService _moraService;
    private readonly FechaSistemaService _fechaSistemaService;

    private const string COMPONENTE_CAPITAL = "CAPITAL";
    private const string COMPONENTE_INTERES = "INTERES";

    public PrestamoEstadoCuentaService(
        AlphaCreditDbContext context,
        PrestamoMoraService moraService,
        FechaSistemaService fechaSistemaService)
    {
        _context = context;
        _moraService = moraService;
        _fechaSistemaService = fechaSistemaService;
    }

    /// <summary>
    /// Obtiene el estado de cuenta completo de un préstamo
    /// </summary>
    public async Task<EstadoCuentaPrestamoDto> ObtenerEstadoCuentaAsync(long prestamoId)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.EstadoPrestamo)
            .Include(p => p.PrestamoComponentes)
            .ThenInclude(pc => pc.ComponentePrestamo)
            .Include(p => p.PrestamoComponentes)
            .ThenInclude(pc => pc.EstadoComponente)
            .FirstOrDefaultAsync(p => p.PrestamoId == prestamoId);

        if (prestamo == null)
            throw new InvalidOperationException($"Préstamo {prestamoId} no encontrado");

        var fechaActual = await _fechaSistemaService.ObtenerFechaActualAsync();

        // Calcular mora actualizada
        await _moraService.ActualizarSaldoMoraPrestamoAsync(prestamoId, fechaActual);

        var estadoCuenta = new EstadoCuentaPrestamoDto
        {
            PrestamoId = prestamo.PrestamoId,
            PrestamoNumero = prestamo.PrestamoNumero,
            NombreCliente = $"{prestamo.Persona.PersonaPrimerNombre} {prestamo.Persona.PersonaPrimerApellido}",
            MontoOriginal = prestamo.PrestamoMonto,
            SaldoCapital = prestamo.PrestamoSaldoCapital,
            SaldoInteres = prestamo.PrestamoSaldoInteres,
            SaldoMora = prestamo.PrestamoSaldoMora,
            SaldoTotal = prestamo.PrestamoSaldoCapital + prestamo.PrestamoSaldoInteres + prestamo.PrestamoSaldoMora,
            FechaDesembolso = prestamo.PrestamoFechaDesembolso,
            FechaVencimiento = prestamo.PrestamoFechaVencimiento,
            EstadoPrestamo = prestamo.EstadoPrestamo.EstadoPrestamoNombre
        };

        // Obtener cuotas agrupadas
        var cuotas = await GenerarDetalleCuotasAsync(prestamo, fechaActual);
        estadoCuenta.Cuotas = cuotas;

        // Generar resumen
        estadoCuenta.Resumen = GenerarResumen(cuotas, prestamo);

        return estadoCuenta;
    }

    private async Task<List<CuotaDetalleDto>> GenerarDetalleCuotasAsync(
        Models.Prestamo prestamo,
        DateTime fechaActual)
    {
        var cuotas = new List<CuotaDetalleDto>();

        // Agrupar componentes por número de cuota
        var componentesPorCuota = prestamo.PrestamoComponentes
            .Where(pc => pc.PrestamoComponenteNumeroCuota.HasValue)
            .GroupBy(pc => pc.PrestamoComponenteNumeroCuota!.Value)
            .OrderBy(g => g.Key);

        // Calcular mora por componente
        var moraPorComponente = await _moraService.CalcularMoraPorComponenteAsync(
            prestamo.PrestamoComponentes.ToList(),
            fechaActual);

        foreach (var grupoCuota in componentesPorCuota)
        {
            var numeroCuota = grupoCuota.Key;
            var componentesCuota = grupoCuota.ToList();

            var componenteCapital = componentesCuota
                .FirstOrDefault(c => c.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_CAPITAL);

            var componenteInteres = componentesCuota
                .FirstOrDefault(c => c.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_INTERES);

            // Usar fecha de vencimiento del capital (o interés si no hay capital)
            var fechaVencimiento = componenteCapital?.PrestamoComponenteFechaVencimiento
                                ?? componenteInteres?.PrestamoComponenteFechaVencimiento;

            int diasVencidos = 0;
            bool estaVencida = false;

            if (fechaVencimiento.HasValue && fechaVencimiento.Value.Date <= fechaActual.Date)
            {
                diasVencidos = (fechaActual.Date - fechaVencimiento.Value.Date).Days;
                estaVencida = diasVencidos > 0;
            }

            // Calcular mora de la cuota
            decimal moraCalculada = componentesCuota
                .Where(c => moraPorComponente.ContainsKey(c.PrestamoComponenteId))
                .Sum(c => moraPorComponente[c.PrestamoComponenteId]);

            var cuota = new CuotaDetalleDto
            {
                NumeroCuota = numeroCuota,
                FechaVencimiento = fechaVencimiento,
                DiasVencidos = diasVencidos,
                EstaVencida = estaVencida,
                Estado = DeterminarEstadoCuota(componentesCuota),

                // Capital
                CapitalComponenteId = componenteCapital?.PrestamoComponenteId,
                CapitalMonto = componenteCapital?.PrestamoComponenteMonto ?? 0,
                CapitalSaldo = componenteCapital?.PrestamoComponenteSaldo ?? 0,
                CapitalPagado = (componenteCapital?.PrestamoComponenteMonto ?? 0) - (componenteCapital?.PrestamoComponenteSaldo ?? 0),

                // Interés
                InteresComponenteId = componenteInteres?.PrestamoComponenteId,
                InteresMonto = componenteInteres?.PrestamoComponenteMonto ?? 0,
                InteresSaldo = componenteInteres?.PrestamoComponenteSaldo ?? 0,
                InteresPagado = (componenteInteres?.PrestamoComponenteMonto ?? 0) - (componenteInteres?.PrestamoComponenteSaldo ?? 0),

                // Mora
                MoraCalculada = moraCalculada,
                MoraPagada = 0 // Por ahora, calcular después si hay registros de pago de mora
            };

            // Totales
            cuota.CuotaTotal = cuota.CapitalMonto + cuota.InteresMonto;
            cuota.SaldoTotal = cuota.CapitalSaldo + cuota.InteresSaldo + cuota.MoraCalculada;
            cuota.PagadoTotal = cuota.CapitalPagado + cuota.InteresPagado + cuota.MoraPagada;

            cuotas.Add(cuota);
        }

        return cuotas;
    }

    private string DeterminarEstadoCuota(List<Models.PrestamoComponente> componentes)
    {
        var totalMonto = componentes.Sum(c => c.PrestamoComponenteMonto);
        var totalSaldo = componentes.Sum(c => c.PrestamoComponenteSaldo);

        if (totalSaldo == 0)
            return "Pagada";

        if (totalSaldo < totalMonto)
            return "Parcial";

        var fechaActual = _fechaSistemaService.ObtenerFechaActual();
        var fechaVencimiento = componentes.FirstOrDefault()?.PrestamoComponenteFechaVencimiento;

        if (fechaVencimiento.HasValue && fechaVencimiento.Value.Date < fechaActual)
            return "Vencida";

        return "Pendiente";
    }

    private ResumenCuotasDto GenerarResumen(List<CuotaDetalleDto> cuotas, Models.Prestamo prestamo)
    {
        var resumen = new ResumenCuotasDto
        {
            TotalCuotas = cuotas.Count,
            CuotasPagadas = cuotas.Count(c => c.Estado == "Pagada"),
            CuotasPendientes = cuotas.Count(c => c.Estado == "Pendiente" || c.Estado == "Parcial"),
            CuotasVencidas = cuotas.Count(c => c.Estado == "Vencida"),
            MontoTotalPagado = cuotas.Sum(c => c.PagadoTotal),
            MontoTotalPendiente = cuotas.Sum(c => c.SaldoTotal),
            MontoMoraTotal = prestamo.PrestamoSaldoMora
        };

        // Próximo pago (primera cuota pendiente)
        var proximaCuota = cuotas
            .Where(c => c.SaldoTotal > 0)
            .OrderBy(c => c.FechaVencimiento)
            .FirstOrDefault();

        if (proximaCuota != null)
        {
            resumen.ProximoPago = proximaCuota.SaldoTotal;
            resumen.FechaProximoPago = proximaCuota.FechaVencimiento;
        }

        return resumen;
    }

    /// <summary>
    /// Obtiene el historial de pagos de un préstamo
    /// </summary>
    public async Task<List<MovimientoPrestamoDto>> ObtenerHistorialPagosAsync(long prestamoId)
    {
        var movimientos = await _context.MovimientosPrestamo
            .Include(m => m.FormaPago)
            .Where(m => m.PrestamoId == prestamoId &&
                       (m.MovimientoPrestamoTipo == "PAGO" ||
                        m.MovimientoPrestamoTipo == "PAGO_PARCIAL" ||
                        m.MovimientoPrestamoTipo == "ABONO_CAPITAL" ||
                        m.MovimientoPrestamoTipo == "PAGO_MORA"))
            .OrderByDescending(m => m.MovimientoPrestamoFecha)
            .Select(m => new MovimientoPrestamoDto
            {
                MovimientoPrestamoId = m.MovimientoPrestamoId,
                Fecha = m.MovimientoPrestamoFecha,
                Tipo = m.MovimientoPrestamoTipo,
                FormaPago = m.FormaPago != null ? m.FormaPago.FormaPagoNombre : "N/A",
                MontoCapital = m.MovimientoPrestamoMontoCapital,
                MontoInteres = m.MovimientoPrestamoMontoInteres,
                MontoMora = m.MovimientoPrestamoMontoMora,
                MontoTotal = m.MovimientoPrestamoMontoTotal,
                Observaciones = m.MovimientoPrestamoObservaciones,
                Usuario = m.MovimientoPrestamoUserCrea
            })
            .ToListAsync();

        return movimientos;
    }
}

public class MovimientoPrestamoDto
{
    public long MovimientoPrestamoId { get; set; }
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string FormaPago { get; set; } = string.Empty;
    public decimal MontoCapital { get; set; }
    public decimal MontoInteres { get; set; }
    public decimal MontoMora { get; set; }
    public decimal MontoTotal { get; set; }
    public string? Observaciones { get; set; }
    public string? Usuario { get; set; }
}
