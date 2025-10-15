using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("parametrossistema")]
public class ParametrosSistema
{
    [Key]
    [Column("parametrosistemaid")]
    public long ParametroSistemaId { get; set; }

    [Column("parametrosistemallave")]
    [Required]
    [MaxLength(100)]
    public string ParametroSistemaLlave { get; set; } = string.Empty;

    [Column("parametrossistemavalor")]
    [Required]
    public string ParametrosSistemaValor { get; set; } = string.Empty;

    [Column("parametrossistemadescripcion")]
    [MaxLength(500)]
    public string? ParametrosSistemaDescripcion { get; set; }

    [Column("parametrossistematipodato")]
    [MaxLength(20)]
    public string? ParametrosSistemaTipoDato { get; set; }

    [Column("parametrossistemaestaactivo")]
    public bool ParametrosSistemaEstaActivo { get; set; }
}
