using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("fechasistema")]
public class FechaSistema
{
    [Key]
    [Column("fechasistemaid")]
    public long FechaSistemaId { get; set; }

    [Column("fechasistemafecha")]
    public DateTime FechaSistemaFecha { get; set; }

    [Column("fechasistemaestaactiva")]
    public bool FechaSistemaEstaActiva { get; set; }

    [Column("fechasistemausercrea")]
    [MaxLength(40)]
    public string? FechaSistemaUserCrea { get; set; }

    [Column("fechasistemafechacreacion")]
    public DateTime FechaSistemaFechaCreacion { get; set; }
    [Column("fechasistemaestacerrado")]
    public bool FechaSistemaEstaCerrado { get; set; }
    [Column("fechasistemaesferiado")]
    public bool FechaSistemaEsFeriado{ get; set; }

}
