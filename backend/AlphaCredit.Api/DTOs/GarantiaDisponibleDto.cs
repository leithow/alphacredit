namespace AlphaCredit.Api.DTOs;

/// <summary>
/// DTO para mostrar garantías con su valor disponible
/// </summary>
public class GarantiaDisponibleDto
{
    public long GarantiaId { get; set; }
    public long PersonaId { get; set; }
    public string PersonaNombreCompleto { get; set; } = string.Empty;
    public long? PersonaFiadorId { get; set; }
    public string? PersonaFiadorNombreCompleto { get; set; }
    public long TipoGarantiaId { get; set; }
    public string TipoGarantiaNombre { get; set; } = string.Empty;
    public string GarantiaDescripcion { get; set; } = string.Empty;
    public decimal? GarantiaValorComercial { get; set; }
    public decimal? GarantiaValorRealizable { get; set; }

    /// <summary>
    /// Valor que ya está comprometido en otros préstamos
    /// </summary>
    public decimal MontoComprometido { get; set; }

    /// <summary>
    /// Valor disponible para comprometer en nuevos préstamos
    /// Calculado como: GarantiaValorRealizable - MontoComprometido
    /// </summary>
    public decimal ValorDisponible { get; set; }

    public string? GarantiaObservaciones { get; set; }
}
