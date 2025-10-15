using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("fondomovimiento")]
public class FondoMovimiento
{
    [Key]
    [Column("fondomovimientoid")]
    public long FondoMovimientoId { get; set; }

    [Column("fondoid")]
    public long FondoId { get; set; }

    [Column("transaccionid")]
    public long? TransaccionId { get; set; }

    [Column("fondomovimientotipo")]
    [Required]
    [MaxLength(20)]
    public string FondoMovimientoTipo { get; set; } = string.Empty;

    [Column("fondomovimientofecha")]
    public DateTime FondoMovimientoFecha { get; set; }

    [Column("fondomovimientomonto", TypeName = "decimal(18,2)")]
    public decimal FondoMovimientoMonto { get; set; }

    [Column("fondomovimientoconcepto")]
    [MaxLength(500)]
    public string? FondoMovimientoConcepto { get; set; }

    [Column("fondomovimientousercrea")]
    [MaxLength(40)]
    public string? FondoMovimientoUserCrea { get; set; }

    [Column("fondomovimientofechacreacion")]
    public DateTime FondoMovimientoFechaCreacion { get; set; }

    // Navigation properties
    [ForeignKey("FondoId")]
    public virtual Fondo Fondo { get; set; } = null!;

    [ForeignKey("TransaccionId")]
    public virtual Transaccion? Transaccion { get; set; }
}
