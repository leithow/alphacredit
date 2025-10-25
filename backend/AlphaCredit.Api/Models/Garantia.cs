using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("garantia")]
public class Garantia
{
    [Key]
    [Column("garantiaid")]
    public long GarantiaId { get; set; }

    [Column("personaid")]
    public long PersonaId { get; set; }

    [Column("tipogarantiaid")]
    public long TipoGarantiaId { get; set; }

    [Column("estadogarantiaid")]
    public long EstadoGarantiaId { get; set; }

    [Column("garantiadescripcion")]
    [Required]
    [MaxLength(500)]
    public string GarantiaDescripcion { get; set; } = string.Empty;

    [Column("garantiavalorcomercial", TypeName = "decimal(18,2)")]
    public decimal? GarantiaValorComercial { get; set; }

    [Column("garantiavalorrealizable", TypeName = "decimal(18,2)")]
    public decimal? GarantiaValorRealizable { get; set; }

    [Column("garantiaobservaciones")]
    public string? GarantiaObservaciones { get; set; }

    [Column("garantiausercrea")]
    [MaxLength(40)]
    public string? GarantiaUserCrea { get; set; }

    [Column("garantiausermodifica")]
    [MaxLength(40)]
    public string? GarantiaUserModifica { get; set; }

    [Column("garantiafechacreacion")]
    public DateTime GarantiaFechaCreacion { get; set; }

    [Column("garantiafechamodifica")]
    public DateTime? GarantiaFechaModifica { get; set; }

    [Column("personafiadorid")]
    public long? PersonaFiadorId { get; set; }

    // Navigation properties
    [ForeignKey("PersonaId")]
    public virtual Persona Persona { get; set; } = null!;

    [ForeignKey("PersonaFiadorId")]
    public virtual Persona? PersonaFiador { get; set; }

    [ForeignKey("TipoGarantiaId")]
    public virtual TipoGarantia TipoGarantia { get; set; } = null!;

    [ForeignKey("EstadoGarantiaId")]
    public virtual EstadoGarantia EstadoGarantia { get; set; } = null!;

    public virtual ICollection<PrestamoGarantia> PrestamoGarantias { get; set; } = new List<PrestamoGarantia>();
}
