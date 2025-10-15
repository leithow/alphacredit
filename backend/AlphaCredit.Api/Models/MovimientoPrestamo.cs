using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("movimientoprestamo")]
public class MovimientoPrestamo
{
    [Key]
    [Column("movimientoprestamoid")]
    public long MovimientoPrestamoId { get; set; }

    [Column("prestamoid")]
    public long PrestamoId { get; set; }

    [Column("transaccionid")]
    public long? TransaccionId { get; set; }

    [Column("formapagoid")]
    public long? FormaPagoId { get; set; }

    [Column("movimientoprestamotipo")]
    [Required]
    [MaxLength(20)]
    public string MovimientoPrestamoTipo { get; set; } = string.Empty;

    [Column("movimientoprestamofecha")]
    public DateTime MovimientoPrestamoFecha { get; set; }

    [Column("movimientoprestamomontocapital", TypeName = "decimal(18,2)")]
    public decimal MovimientoPrestamoMontoCapital { get; set; }

    [Column("movimientoprestamomontointeres", TypeName = "decimal(18,2)")]
    public decimal MovimientoPrestamoMontoInteres { get; set; }

    [Column("movimientoprestamomontomora", TypeName = "decimal(18,2)")]
    public decimal MovimientoPrestamoMontoMora { get; set; }

    [Column("movimientoprestamomontootros", TypeName = "decimal(18,2)")]
    public decimal MovimientoPrestamoMontoOtros { get; set; }

    [Column("movimientoprestamomontototal", TypeName = "decimal(18,2)")]
    public decimal MovimientoPrestamoMontoTotal { get; set; }

    [Column("movimientoprestamoobservaciones")]
    public string? MovimientoPrestamoObservaciones { get; set; }

    [Column("movimientoprestamousercrea")]
    [MaxLength(40)]
    public string? MovimientoPrestamoUserCrea { get; set; }

    [Column("movimientoprestamofechacreacion")]
    public DateTime MovimientoPrestamoFechaCreacion { get; set; }

    // Navigation properties
    [ForeignKey("PrestamoId")]
    public virtual Prestamo Prestamo { get; set; } = null!;

    [ForeignKey("TransaccionId")]
    public virtual Transaccion? Transaccion { get; set; }

    [ForeignKey("FormaPagoId")]
    public virtual FormaPago? FormaPago { get; set; }
}
