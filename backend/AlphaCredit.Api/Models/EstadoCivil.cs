using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("estadocivil")]
public class EstadoCivil
{
    [Key]
    [Column("estadocivilid")]
    public long EstadoCivilId { get; set; }

    [Column("estadocivilnombre")]
    [Required]
    [MaxLength(40)]
    public string EstadoCivilNombre { get; set; } = string.Empty;

    [Column("estadocivildescripcion")]
    [MaxLength(200)]
    public string? EstadoCivilDescripcion { get; set; }

    [Column("estadocivilestaactivo")]
    public bool EstadoCivilEstaActivo { get; set; }

    [Column("estadocivilrequiereconyuge")]
    public bool EstadoCivilRequiereConyuge { get; set; }

    // Navigation properties
    public virtual ICollection<Persona> Personas { get; set; } = new List<Persona>();
}
