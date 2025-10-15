using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("ubicaciongeografica")]
public class UbicacionGeografica
{
    [Key]
    [Column("ubicaciongeograficaid")]
    public long UbicacionGeograficaId { get; set; }

    [Column("paisid")]
    public long? PaisId { get; set; }

    [Column("ubicaciongeograficapadre")]
    public long? UbicacionGeograficaPadre { get; set; }

    [Column("ubicaciongeograficanombre")]
    [Required]
    [MaxLength(100)]
    public string UbicacionGeograficaNombre { get; set; } = string.Empty;

    [Column("ubicaciongeograficanivel")]
    public short UbicacionGeograficaNivel { get; set; }

    [Column("ubicaciongeograficacodigo")]
    [MaxLength(20)]
    public string? UbicacionGeograficaCodigo { get; set; }

    [Column("ubicaciongeograficaestaactiva")]
    public bool UbicacionGeograficaEstaActiva { get; set; }

    // Navigation properties
    [ForeignKey("PaisId")]
    public virtual Pais? Pais { get; set; }

    [ForeignKey("UbicacionGeograficaPadre")]
    public virtual UbicacionGeografica? UbicacionPadre { get; set; }

    public virtual ICollection<UbicacionGeografica> UbicacionesHijas { get; set; } = new List<UbicacionGeografica>();
}
