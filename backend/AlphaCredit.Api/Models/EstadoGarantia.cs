using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("estadogarantia")]
public class EstadoGarantia
{
    [Key]
    [Column("estadogarantiaid")]
    public long EstadoGarantiaId { get; set; }

    [Column("estadogarantianombre")]
    [Required]
    [MaxLength(40)]
    public string EstadoGarantiaNombre { get; set; } = string.Empty;

    [Column("estadogarantiadescripcion")]
    [MaxLength(200)]
    public string? EstadoGarantiaDescripcion { get; set; }

    [Column("estadogarantiaestaactiva")]
    public bool EstadoGarantiaEstaActiva { get; set; }

    // Navigation properties
    public virtual ICollection<Garantia> Garantias { get; set; } = new List<Garantia>();
}
