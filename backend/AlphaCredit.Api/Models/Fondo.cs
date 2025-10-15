using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("fondo")]
public class Fondo
{
    [Key]
    [Column("fondoid")]
    public long FondoId { get; set; }

    [Column("tipofondoid")]
    public long TipoFondoId { get; set; }

    [Column("monedaid")]
    public long MonedaId { get; set; }

    [Column("fondonombre")]
    [Required]
    [MaxLength(100)]
    public string FondoNombre { get; set; } = string.Empty;

    [Column("fondodescripcion")]
    [MaxLength(300)]
    public string? FondoDescripcion { get; set; }

    [Column("fondosaldoinicial", TypeName = "decimal(18,2)")]
    public decimal FondoSaldoInicial { get; set; }

    [Column("fondosaldoactual", TypeName = "decimal(18,2)")]
    public decimal FondoSaldoActual { get; set; }

    [Column("fondoestaactivo")]
    public bool FondoEstaActivo { get; set; }

    [Column("fondousercrea")]
    [MaxLength(40)]
    public string? FondoUserCrea { get; set; }

    [Column("fondousermodifica")]
    [MaxLength(40)]
    public string? FondoUserModifica { get; set; }

    [Column("fondofechacreacion")]
    public DateTime FondoFechaCreacion { get; set; }

    [Column("fondofechamodifica")]
    public DateTime? FondoFechaModifica { get; set; }

    // Navigation properties
    [ForeignKey("TipoFondoId")]
    public virtual TipoFondo TipoFondo { get; set; } = null!;

    [ForeignKey("MonedaId")]
    public virtual Moneda Moneda { get; set; } = null!;

    public virtual ICollection<FondoMovimiento> FondoMovimientos { get; set; } = new List<FondoMovimiento>();
}
