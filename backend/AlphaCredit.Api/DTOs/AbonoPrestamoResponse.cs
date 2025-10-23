namespace AlphaCredit.Api.DTOs;

/// <summary>
/// Response después de aplicar un abono
/// </summary>
public class AbonoPrestamoResponse
{
    public long MovimientoPrestamoId { get; set; }
    public decimal MontoAplicado { get; set; }
    public decimal MontoCapital { get; set; }
    public decimal MontoInteres { get; set; }
    public decimal MontoMora { get; set; }
    public decimal MontoOtros { get; set; }
    public DateTime FechaAplicacion { get; set; }
    public List<ComponenteAfectadoDto> ComponentesAfectados { get; set; } = new();
    public SaldosPrestamoDto SaldosNuevos { get; set; } = new();
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>
/// Componente afectado por el abono
/// </summary>
public class ComponenteAfectadoDto
{
    public long? PrestamoComponenteId { get; set; }
    public string ComponenteNombre { get; set; } = string.Empty;
    public int? NumeroCuota { get; set; }
    public decimal MontoAntes { get; set; }
    public decimal MontoAplicado { get; set; }
    public decimal SaldoNuevo { get; set; }
    public string EstadoNuevo { get; set; } = string.Empty;
}

/// <summary>
/// Saldos actualizados del préstamo
/// </summary>
public class SaldosPrestamoDto
{
    public decimal SaldoCapital { get; set; }
    public decimal SaldoInteres { get; set; }
    public decimal SaldoMora { get; set; }
    public decimal SaldoTotal { get; set; }
    public int CuotasPendientes { get; set; }
    public int CuotasPagadas { get; set; }
}
