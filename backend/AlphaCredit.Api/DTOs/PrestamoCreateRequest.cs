using System.ComponentModel.DataAnnotations;

namespace AlphaCredit.Api.DTOs;

/// <summary>
/// DTO para crear un nuevo préstamo
/// </summary>
public class PrestamoCreateRequest
{
    [Required(ErrorMessage = "La persona es requerida")]
    public long PersonaId { get; set; }

    [Required(ErrorMessage = "La sucursal es requerida")]
    public long SucursalId { get; set; }

    [Required(ErrorMessage = "La moneda es requerida")]
    public long MonedaId { get; set; }

    /// <summary>
    /// Estado del préstamo (opcional, se asigna automáticamente si no se proporciona)
    /// </summary>
    public long? EstadoPrestamoId { get; set; }

    [Required(ErrorMessage = "La frecuencia de pago es requerida")]
    public long FrecuenciaPagoId { get; set; }

    public long? DestinoCreditoId { get; set; }

    [Required(ErrorMessage = "El fondo es requerido")]
    public long FondoId { get; set; }

    [Required(ErrorMessage = "El monto del préstamo es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
    public decimal PrestamoMonto { get; set; }

    [Required(ErrorMessage = "La tasa de interés es requerida")]
    [Range(0, 100, ErrorMessage = "La tasa de interés debe estar entre 0 y 100")]
    public decimal PrestamoTasaInteres { get; set; }

    [Required(ErrorMessage = "El plazo (número de cuotas) es requerido")]
    [Range(1, int.MaxValue, ErrorMessage = "El plazo debe ser al menos 1 cuota")]
    public int PrestamoPlazo { get; set; }

    /// <summary>
    /// Indica si el préstamo es al vencimiento (una sola cuota al final)
    /// Si es false, se generará una tabla de amortización con cuotas periódicas
    /// </summary>
    public bool EsAlVencimiento { get; set; } = false;

    public DateTime? PrestamoFechaAprobacion { get; set; }

    public DateTime? PrestamoFechaDesembolso { get; set; }

    [MaxLength(500)]
    public string? PrestamoObservaciones { get; set; }

    [MaxLength(40)]
    public string? PrestamoUserCrea { get; set; }
}
