using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("actividadeconomica")]
public class ActividadEconomica
{
    [Key]
    [Column("actividadeconomicaid")]
    public long ActividadEconomicaId { get; set; }

    [Column("actividadcnbsid")]
    public long? ActividadCnbsId { get; set; }

    [Column("actividadeconomicanombre")]
    [Required]
    [MaxLength(200)]
    public string ActividadEconomicaNombre { get; set; } = string.Empty;

    [Column("actividadeconomicadescripcion")]
    [MaxLength(500)]
    public string? ActividadEconomicaDescripcion { get; set; }

    [Column("actividadeconomicaestaactiva")]
    public bool ActividadEconomicaEstaActiva { get; set; }

    // Navigation properties
    [ForeignKey("ActividadCnbsId")]
    public virtual ActividadCnbs? ActividadCnbs { get; set; }

    public virtual ICollection<PersonaActividad> PersonaActividades { get; set; } = new List<PersonaActividad>();
}
