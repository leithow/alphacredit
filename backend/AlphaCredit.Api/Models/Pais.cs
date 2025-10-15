using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("pais")]
public class Pais
{
    [Key]
    [Column("paisid")]
    public long PaisId { get; set; }

    [Column("paisnombre")]
    [Required]
    [MaxLength(100)]
    public string PaisNombre { get; set; } = string.Empty;

    [Column("paiscodigo")]
    [MaxLength(5)]
    public string? PaisCodigo { get; set; }

    [Column("paisestaactivo")]
    public bool PaisEstaActivo { get; set; }

    // Navigation properties
    public virtual ICollection<UbicacionGeografica> UbicacionesGeograficas { get; set; } = new List<UbicacionGeografica>();
}
