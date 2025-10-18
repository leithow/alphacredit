using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("personaconyuge")]
public class PersonaConyuge
{
    [Key]
    [Column("personaconyugeid")]
    public long PersonaConyugeId { get; set; }

    [Column("personaid")]
    public long PersonaId { get; set; }

    [Column("conyugenombre")]
    [Required]
    [MaxLength(120)]
    public string ConyugeNombre { get; set; } = string.Empty;

    [Column("conyugetelefono")]
    [MaxLength(20)]
    public string? ConyugeTelefono { get; set; }

    [Column("conyugefechacreacion")]
    public DateTime ConyugeFechaCreacion { get; set; }

    [Column("conyugefechamodifica")]
    public DateTime? ConyugeFechaModifica { get; set; }

    // Navigation properties
    [ForeignKey("PersonaId")]
    public virtual Persona Persona { get; set; } = null!;
}
