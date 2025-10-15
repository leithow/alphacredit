using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("destinocredito")]
public class DestinoCredito
{
    [Key]
    [Column("destinocreditoid")]
    public long DestinoCreditoId { get; set; }

    [Column("destinocreditonombre")]
    [Required]
    [MaxLength(100)]
    public string DestinoCreditoNombre { get; set; } = string.Empty;

    [Column("destinocreditodescripcion")]
    [MaxLength(300)]
    public string? DestinoCreditoDescripcion { get; set; }

    [Column("destinocreditoestaactivo")]
    public bool DestinoCreditoEstaActivo { get; set; }

    // Navigation properties
    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
