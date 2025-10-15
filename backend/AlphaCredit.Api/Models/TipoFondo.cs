using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("tipofondo")]
public class TipoFondo
{
    [Key]
    [Column("tipofondoid")]
    public long TipoFondoId { get; set; }

    [Column("tipofondonombre")]
    [Required]
    [MaxLength(60)]
    public string TipoFondoNombre { get; set; } = string.Empty;

    [Column("tipofondodescripcion")]
    [MaxLength(200)]
    public string? TipoFondoDescripcion { get; set; }

    [Column("tipofondoestaactivo")]
    public bool TipoFondoEstaActivo { get; set; }

    // Navigation properties
    public virtual ICollection<Fondo> Fondos { get; set; } = new List<Fondo>();
}
