using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("tipogarantia")]
public class TipoGarantia
{
    [Key]
    [Column("tipogarantiaid")]
    public long TipoGarantiaId { get; set; }

    [Column("tipogarantianombre")]
    [Required]
    [MaxLength(60)]
    public string TipoGarantiaNombre { get; set; } = string.Empty;

    [Column("tipogarantiadescripcion")]
    [MaxLength(200)]
    public string? TipoGarantiaDescripcion { get; set; }

    [Column("tipogarantiaestaactiva")]
    public bool TipoGarantiaEstaActiva { get; set; }

    // Navigation properties
    public virtual ICollection<Garantia> Garantias { get; set; } = new List<Garantia>();
}
