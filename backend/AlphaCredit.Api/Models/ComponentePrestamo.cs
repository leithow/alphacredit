using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("componenteprestamo")]
public class ComponentePrestamo
{
    [Key]
    [Column("componenteprestamoid")]
    public long ComponentePrestamoId { get; set; }

    [Column("componenteprestamonombre")]
    [Required]
    [MaxLength(60)]
    public string ComponentePrestamoNombre { get; set; } = string.Empty;

    [Column("componenteprestamodescripcion")]
    [MaxLength(200)]
    public string? ComponentePrestamoDescripcion { get; set; }

    [Column("componenteprestamotipo")]
    [MaxLength(20)]
    public string? ComponentePrestamoTipo { get; set; }

    [Column("componenteprestamoestaactivo")]
    public bool ComponentePrestamoEstaActivo { get; set; }

    // Navigation properties
    public virtual ICollection<PrestamoComponente> PrestamoComponentes { get; set; } = new List<PrestamoComponente>();
}
