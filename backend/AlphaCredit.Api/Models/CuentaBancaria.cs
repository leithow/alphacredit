using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("cuentabancaria")]
public class CuentaBancaria
{
    [Key]
    [Column("cuentabancariaid")]
    public long CuentaBancariaId { get; set; }

    [Column("bancoid")]
    public long BancoId { get; set; }

    [Column("tipocuentaid")]
    public long TipoCuentaId { get; set; }

    [Column("monedaid")]
    public long MonedaId { get; set; }

    [Column("cuentabancarianumero")]
    [Required]
    [MaxLength(40)]
    public string CuentaBancariaNumero { get; set; } = string.Empty;

    [Column("cuentabancarianombre")]
    [MaxLength(100)]
    public string? CuentaBancariaNombre { get; set; }

    [Column("cuentabancariasaldo", TypeName = "decimal(18,2)")]
    public decimal CuentaBancariaSaldo { get; set; }

    [Column("cuentabancariaestaactiva")]
    public bool CuentaBancariaEstaActiva { get; set; }

    // Navigation properties
    [ForeignKey("BancoId")]
    public virtual Banco Banco { get; set; } = null!;

    [ForeignKey("TipoCuentaId")]
    public virtual TipoCuenta TipoCuenta { get; set; } = null!;

    [ForeignKey("MonedaId")]
    public virtual Moneda Moneda { get; set; } = null!;

    public virtual ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
}
