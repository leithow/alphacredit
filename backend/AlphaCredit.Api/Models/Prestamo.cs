using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("prestamo")]
public class Prestamo
{
    [Key]
    [Column("prestamoid")]
    public long PrestamoId { get; set; }

    [Column("personaid")]
    public long PersonaId { get; set; }

    [Column("sucursalid")]
    public long SucursalId { get; set; }

    [Column("monedaid")]
    public long MonedaId { get; set; }

    [Column("estadoprestamoid")]
    public long EstadoPrestamoId { get; set; }

    [Column("destinocreditoid")]
    public long? DestinoCreditoId { get; set; }

    [Column("frecuenciapagoid")]
    public long FrecuenciaPagoId { get; set; }

    [Column("prestamonumero")]
    [Required]
    [MaxLength(20)]
    public string PrestamoNumero { get; set; } = string.Empty;

    [Column("prestamomonto", TypeName = "decimal(18,2)")]
    public decimal PrestamoMonto { get; set; }

    [Column("prestamotasainteres", TypeName = "decimal(5,2)")]
    public decimal PrestamoTasaInteres { get; set; }

    [Column("prestamoplazo")]
    public int PrestamoPlazo { get; set; }

    [Column("prestamofechaaprobacion")]
    public DateTime? PrestamoFechaAprobacion { get; set; }

    [Column("prestamofechadesembolso")]
    public DateTime? PrestamoFechaDesembolso { get; set; }

    [Column("prestamofechavencimiento")]
    public DateTime? PrestamoFechaVencimiento { get; set; }

    [Column("prestamosaldocapital", TypeName = "decimal(18,2)")]
    public decimal PrestamoSaldoCapital { get; set; }

    [Column("prestamosaldointeres", TypeName = "decimal(18,2)")]
    public decimal PrestamoSaldoInteres { get; set; }

    [Column("prestamosaldomora", TypeName = "decimal(18,2)")]
    public decimal PrestamoSaldoMora { get; set; }

    [Column("prestamoobservaciones")]
    public string? PrestamoObservaciones { get; set; }

    [Column("prestamousercrea")]
    [MaxLength(40)]
    public string? PrestamoUserCrea { get; set; }

    [Column("prestamousermodifica")]
    [MaxLength(40)]
    public string? PrestamoUserModifica { get; set; }

    [Column("prestamofechacreacion")]
    public DateTime PrestamoFechaCreacion { get; set; }

    [Column("prestamofechamodifica")]
    public DateTime? PrestamoFechaModifica { get; set; }

    // Navigation properties
    [ForeignKey("PersonaId")]
    public virtual Persona Persona { get; set; } = null!;

    [ForeignKey("SucursalId")]
    public virtual Sucursal Sucursal { get; set; } = null!;

    [ForeignKey("MonedaId")]
    public virtual Moneda Moneda { get; set; } = null!;

    [ForeignKey("EstadoPrestamoId")]
    public virtual EstadoPrestamo EstadoPrestamo { get; set; } = null!;

    [ForeignKey("DestinoCreditoId")]
    public virtual DestinoCredito? DestinoCredito { get; set; }

    [ForeignKey("FrecuenciaPagoId")]
    public virtual FrecuenciaPago FrecuenciaPago { get; set; } = null!;

    public virtual ICollection<PrestamoComponente> PrestamoComponentes { get; set; } = new List<PrestamoComponente>();
    public virtual ICollection<PrestamoGarantia> PrestamoGarantias { get; set; } = new List<PrestamoGarantia>();
    public virtual ICollection<MovimientoPrestamo> MovimientosPrestamo { get; set; } = new List<MovimientoPrestamo>();
}
