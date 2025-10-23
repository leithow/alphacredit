namespace AlphaCredit.Api.DTOs;

/// <summary>
/// Estado de cuenta completo de un préstamo con detalle de cuotas
/// </summary>
public class EstadoCuentaPrestamoDto
{
    public long PrestamoId { get; set; }
    public string PrestamoNumero { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public decimal MontoOriginal { get; set; }
    public decimal SaldoCapital { get; set; }
    public decimal SaldoInteres { get; set; }
    public decimal SaldoMora { get; set; }
    public decimal SaldoTotal { get; set; }
    public DateTime? FechaDesembolso { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string EstadoPrestamo { get; set; } = string.Empty;

    public List<CuotaDetalleDto> Cuotas { get; set; } = new();
    public ResumenCuotasDto Resumen { get; set; } = new();
}

/// <summary>
/// Detalle de una cuota con sus componentes
/// </summary>
public class CuotaDetalleDto
{
    public int NumeroCuota { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int DiasVencidos { get; set; }
    public bool EstaVencida { get; set; }
    public string Estado { get; set; } = string.Empty;

    // Capital
    public long? CapitalComponenteId { get; set; }
    public decimal CapitalMonto { get; set; }
    public decimal CapitalSaldo { get; set; }
    public decimal CapitalPagado { get; set; }

    // Interés
    public long? InteresComponenteId { get; set; }
    public decimal InteresMonto { get; set; }
    public decimal InteresSaldo { get; set; }
    public decimal InteresPagado { get; set; }

    // Mora (si aplica)
    public decimal MoraCalculada { get; set; }
    public decimal MoraPagada { get; set; }

    // Totales
    public decimal CuotaTotal { get; set; }
    public decimal SaldoTotal { get; set; }
    public decimal PagadoTotal { get; set; }
}

/// <summary>
/// Resumen de cuotas por estado
/// </summary>
public class ResumenCuotasDto
{
    public int TotalCuotas { get; set; }
    public int CuotasPagadas { get; set; }
    public int CuotasPendientes { get; set; }
    public int CuotasVencidas { get; set; }
    public decimal MontoTotalPagado { get; set; }
    public decimal MontoTotalPendiente { get; set; }
    public decimal MontoMoraTotal { get; set; }
    public decimal ProximoPago { get; set; }
    public DateTime? FechaProximoPago { get; set; }
}
