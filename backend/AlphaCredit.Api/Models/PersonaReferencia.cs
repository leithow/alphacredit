using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("personareferencia")]
public class PersonaReferencia
{
    [Key]
    [Column("personareferenciaid")]
    public long PersonaReferenciaId { get; set; }

    [Column("personaid")]
    public long PersonaId { get; set; }

    [Column("personareferencianombre")]
    [Required]
    [MaxLength(120)]
    public string PersonaReferenciaNombre { get; set; } = string.Empty;

    [Column("personareferenciatelefono")]
    [MaxLength(20)]
    public string? PersonaReferenciaTelefono { get; set; }

    [Column("personareferenciadireccion")]
    [MaxLength(500)]
    public string? PersonaReferenciaDireccion { get; set; }

    [Column("personareferenciaparentesco")]
    [MaxLength(40)]
    public string? PersonaReferenciaParentesco { get; set; }

    [Column("personareferenciaestaactiva")]
    public bool PersonaReferenciaEstaActiva { get; set; }

    // Navigation properties
    [ForeignKey("PersonaId")]
    public virtual Persona Persona { get; set; } = null!;
}
