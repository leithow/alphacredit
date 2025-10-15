using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("formapago")]
public class FormaPago
{
    [Key]
    [Column("formapagoid")]
    public long FormaPagoId { get; set; }

    [Column("formapaganombre")]
    [Required]
    [MaxLength(60)]
    public string FormaPagoNombre { get; set; } = string.Empty;

    [Column("formapagodescripcion")]
    [MaxLength(200)]
    public string? FormaPagoDescripcion { get; set; }

    [Column("formapagoestaactiva")]
    public bool FormaPagoEstaActiva { get; set; }

    // Navigation properties
    public virtual ICollection<MovimientoPrestamo> MovimientosPrestamo { get; set; } = new List<MovimientoPrestamo>();
}
