using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("personaactividad")]
public class PersonaActividad
{
    [Key]
    [Column("personaactividadid")]
    public long PersonaActividadId { get; set; }

    [Column("personaid")]
    public long PersonaId { get; set; }

    [Column("actividadeconomicaid")]
    public long ActividadEconomicaId { get; set; }

    [Column("personaactividadingresomensual", TypeName = "decimal(18,2)")]
    public decimal? PersonaActividadIngresoMensual { get; set; }

    [Column("personaactividadestaactiva")]
    public bool PersonaActividadEstaActiva { get; set; }

    [Column("personaactividadfechainicio")]
    public DateTime? PersonaActividadFechaInicio { get; set; }

    [Column("personaactividadesprincipal")]
    public bool PersonaActividadEsPrincipal { get; set; }

    // Navigation properties
    [ForeignKey("PersonaId")]
    public virtual Persona Persona { get; set; } = null!;

    [ForeignKey("ActividadEconomicaId")]
    public virtual ActividadEconomica ActividadEconomica { get; set; } = null!;
}
