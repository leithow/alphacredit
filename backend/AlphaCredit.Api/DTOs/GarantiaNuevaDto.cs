using System.ComponentModel.DataAnnotations;

namespace AlphaCredit.Api.DTOs;

/// <summary>
/// DTO para crear nuevas garantías junto con un préstamo
/// </summary>
public class GarantiaNuevaDto
{
    [Required(ErrorMessage = "La persona es requerida")]
    public long PersonaId { get; set; }

    [Required(ErrorMessage = "El tipo de garantía es requerido")]
    public long TipoGarantiaId { get; set; }

    [Required(ErrorMessage = "El estado de la garantía es requerido")]
    public long EstadoGarantiaId { get; set; }

    /// <summary>
    /// ID de la persona fiadora (requerido para garantías fiduciarias)
    /// </summary>
    public long? PersonaFiadorId { get; set; }

    [Required(ErrorMessage = "La descripción es requerida")]
    [MaxLength(500)]
    public string GarantiaDescripcion { get; set; } = string.Empty;

    public decimal? GarantiaValorComercial { get; set; }

    public decimal? GarantiaValorRealizable { get; set; }

    [MaxLength(500)]
    public string? GarantiaObservaciones { get; set; }

    [MaxLength(40)]
    public string? GarantiaUserCrea { get; set; }

    /// <summary>
    /// Monto que se comprometerá de esta garantía para el préstamo
    /// </summary>
    [Required(ErrorMessage = "El monto comprometido es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto comprometido debe ser mayor a cero")]
    public decimal MontoComprometido { get; set; }

    /// <summary>
    /// Nombre del tipo de garantía (usado para referencia en el frontend)
    /// </summary>
    public string? TipoGarantiaNombre { get; set; }
}
