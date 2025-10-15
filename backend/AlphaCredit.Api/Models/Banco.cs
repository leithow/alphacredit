using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("banco")]
public class Banco
{
    [Key]
    [Column("bancoid")]
    public long BancoId { get; set; }

    [Column("banconombre")]
    [Required]
    [MaxLength(100)]
    public string BancoNombre { get; set; } = string.Empty;

    [Column("bancocodigo")]
    [MaxLength(20)]
    public string? BancoCodigo { get; set; }

    [Column("bancoestaactivo")]
    public bool BancoEstaActivo { get; set; }

    // Navigation properties
    public virtual ICollection<CuentaBancaria> CuentasBancarias { get; set; } = new List<CuentaBancaria>();
}
