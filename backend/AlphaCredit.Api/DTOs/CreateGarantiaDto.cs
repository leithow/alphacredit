using System.ComponentModel.DataAnnotations;

namespace AlphaCredit.Api.DTOs;

public class CreateGarantiaDto
{
    [Required]
    public long PersonaId { get; set; }

    [Required]
    public long TipoGarantiaId { get; set; }

    [Required]
    public long EstadoGarantiaId { get; set; }

    public long? PersonaFiadorId { get; set; }

    [Required]
    [MaxLength(500)]
    public string GarantiaDescripcion { get; set; } = string.Empty;

    public decimal? GarantiaValorComercial { get; set; }

    public decimal? GarantiaValorRealizable { get; set; }

    public string? GarantiaObservaciones { get; set; }

    public string? GarantiaUserCrea { get; set; }
}
