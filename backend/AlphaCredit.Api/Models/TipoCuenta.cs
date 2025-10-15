using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("tipocuenta")]
public class TipoCuenta
{
    [Key]
    [Column("tipocuentaid")]
    public long TipoCuentaId { get; set; }

    [Column("tipocuentanombre")]
    [Required]
    [MaxLength(60)]
    public string TipoCuentaNombre { get; set; } = string.Empty;

    [Column("tipocuentadescripcion")]
    [MaxLength(200)]
    public string? TipoCuentaDescripcion { get; set; }

    [Column("tipocuentaestaactiva")]
    public bool TipoCuentaEstaActiva { get; set; }

    // Navigation properties
    public virtual ICollection<CuentaBancaria> CuentasBancarias { get; set; } = new List<CuentaBancaria>();
}
