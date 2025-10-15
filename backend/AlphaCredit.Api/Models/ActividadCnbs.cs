using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("actividadcnbs")]
public class ActividadCnbs
{
    [Key]
    [Column("actividadcnbsid")]
    public long ActividadCnbsId { get; set; }

    [Column("actividadcnbscodigo")]
    [Required]
    [MaxLength(20)]
    public string ActividadCnbsCodigo { get; set; } = string.Empty;

    [Column("actividadcnbsnombre")]
    [Required]
    [MaxLength(200)]
    public string ActividadCnbsNombre { get; set; } = string.Empty;

    [Column("actividadcnbsdescripcion")]
    [MaxLength(500)]
    public string? ActividadCnbsDescripcion { get; set; }

    [Column("actividadcnbsestaactiva")]
    public bool ActividadCnbsEstaActiva { get; set; }

    // Navigation properties
    public virtual ICollection<ActividadEconomica> ActividadesEconomicas { get; set; } = new List<ActividadEconomica>();
}
