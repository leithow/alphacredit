using System.ComponentModel.DataAnnotations;

namespace AlphaCredit.Api.DTOs;

/// <summary>
/// Request para registrar un abono a un préstamo
/// </summary>
public class AbonoPrestamoRequest
{
    [Required]
    public long PrestamoId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    [Required]
    public long? FormaPagoId { get; set; }

    [Required]
    [MaxLength(20)]
    public string TipoAbono { get; set; } = string.Empty; // "CUOTA", "PARCIAL", "CAPITAL", "MORA"

    /// <summary>
    /// Número de cuota específica (opcional, para abonos a cuota específica)
    /// </summary>
    public int? NumeroCuota { get; set; }

    /// <summary>
    /// Si es abono parcial, distribución del monto
    /// </summary>
    public DistribucionAbonoDto? Distribucion { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public DateTime? FechaPago { get; set; }
}

/// <summary>
/// Distribución manual del abono (para abonos parciales personalizados)
/// </summary>
public class DistribucionAbonoDto
{
    public decimal? MontoCapital { get; set; }
    public decimal? MontoInteres { get; set; }
    public decimal? MontoMora { get; set; }
    public decimal? MontoOtros { get; set; }
}
