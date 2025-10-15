using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("transaccion")]
public class Transaccion
{
    [Key]
    [Column("transaccionid")]
    public long TransaccionId { get; set; }

    [Column("cuentabancariaid")]
    public long? CuentaBancariaId { get; set; }

    [Column("transaccionnumero")]
    [MaxLength(40)]
    public string? TransaccionNumero { get; set; }

    [Column("transacciontipo")]
    [Required]
    [MaxLength(20)]
    public string TransaccionTipo { get; set; } = string.Empty;

    [Column("transaccionfecha")]
    public DateTime TransaccionFecha { get; set; }

    [Column("transaccionmonto", TypeName = "decimal(18,2)")]
    public decimal TransaccionMonto { get; set; }

    [Column("transaccionconcepto")]
    [MaxLength(500)]
    public string? TransaccionConcepto { get; set; }

    [Column("transaccionreferencia")]
    [MaxLength(100)]
    public string? TransaccionReferencia { get; set; }

    [Column("transaccionusercrea")]
    [MaxLength(40)]
    public string? TransaccionUserCrea { get; set; }

    [Column("transaccionfechacreacion")]
    public DateTime TransaccionFechaCreacion { get; set; }

    // Navigation properties
    [ForeignKey("CuentaBancariaId")]
    public virtual CuentaBancaria? CuentaBancaria { get; set; }

    public virtual ICollection<MovimientoPrestamo> MovimientosPrestamo { get; set; } = new List<MovimientoPrestamo>();
    public virtual ICollection<FondoMovimiento> FondoMovimientos { get; set; } = new List<FondoMovimiento>();
}
