using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("sexo")]
public class Sexo
{
    [Key]
    [Column("sexoid")]
    public long SexoId { get; set; }

    [Column("sexonombre")]
    [Required]
    [MaxLength(20)]
    public string SexoNombre { get; set; } = string.Empty;

    [Column("sexodescripcion")]
    [MaxLength(100)]
    public string? SexoDescripcion { get; set; }

    [Column("sexoestaactivo")]
    public bool SexoEstaActivo { get; set; }

    // Navigation properties
    public virtual ICollection<Persona> Personas { get; set; } = new List<Persona>();
}
