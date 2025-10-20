using System.ComponentModel.DataAnnotations;

namespace AlphaCredit.Api.DTOs;

/// <summary>
/// DTO para asignar garantías a un préstamo
/// </summary>
public class PrestamoGarantiaDto
{
    [Required(ErrorMessage = "El ID de la garantía es requerido")]
    public long GarantiaId { get; set; }

    [Required(ErrorMessage = "El monto comprometido es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto comprometido debe ser mayor a cero")]
    public decimal MontoComprometido { get; set; }
}
