using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("frecuenciapago")]
public class FrecuenciaPago
{
    [Key]
    [Column("frecuenciapagoid")]
    public long FrecuenciaPagoId { get; set; }

    [Column("frecuenciapaganombre")]
    [Required]
    [MaxLength(40)]
    public string FrecuenciaPagoNombre { get; set; } = string.Empty;

    [Column("frecuenciapagodias")]
    public short FrecuenciaPagoDias { get; set; }

    [Column("frecuenciapagoestaactiva")]
    public bool FrecuenciaPagoEstaActiva { get; set; }

    // Navigation properties
    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
