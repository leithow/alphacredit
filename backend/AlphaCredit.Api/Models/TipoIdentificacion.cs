using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("tipoidentificacion")]
public class TipoIdentificacion
{
    [Key]
    [Column("tipoidentificacionid")]
    public long TipoIdentificacionId { get; set; }

    [Column("tipoidentificacionnombre")]
    [Required]
    [MaxLength(40)]
    public string TipoIdentificacionNombre { get; set; } = string.Empty;

    [Column("tipoidentificaciondescripcion")]
    [MaxLength(200)]
    public string? TipoIdentificacionDescripcion { get; set; }

    [Column("tipoidentificacionestaactivo")]
    public bool TipoIdentificacionEstaActivo { get; set; }

    // Navigation properties
    public virtual ICollection<Persona> Personas { get; set; } = new List<Persona>();
}
