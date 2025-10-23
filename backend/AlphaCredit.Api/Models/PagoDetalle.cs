using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

/// <summary>
/// Detalle de pagos aplicados a componentes específicos de cuotas
/// </summary>
[Table("pagodetalle")]
public class PagoDetalle
{
    [Key]
    [Column("pagodetalleid")]
    public long PagoDetalleId { get; set; }

    [Column("movimientoprestamoid")]
    public long MovimientoPrestamoId { get; set; }

    [Column("prestamocomponenteid")]
    public long? PrestamoComponenteId { get; set; }

    [Column("componenteprestamoid")]
    public long ComponentePrestamoId { get; set; }

    [Column("pagodetallecuotanumero")]
    public int? PagoDetalleCuotaNumero { get; set; }

    [Column("pagodetallemontoaplicado", TypeName = "decimal(18,2)")]
    public decimal PagoDetalleMontoAplicado { get; set; }

    [Column("pagodetallemontoantes", TypeName = "decimal(18,2)")]
    public decimal PagoDetalleMontoAntes { get; set; }

    [Column("pagodetallefechaaplicacion")]
    public DateTime PagoDetalleFechaAplicacion { get; set; }

    // Navigation properties
    [ForeignKey("MovimientoPrestamoId")]
    public virtual MovimientoPrestamo MovimientoPrestamo { get; set; } = null!;

    [ForeignKey("PrestamoComponenteId")]
    public virtual PrestamoComponente? PrestamoComponente { get; set; }

    [ForeignKey("ComponentePrestamoId")]
    public virtual ComponentePrestamo ComponentePrestamo { get; set; } = null!;
}
