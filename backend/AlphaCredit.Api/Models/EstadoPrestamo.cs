using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("estadoprestamo")]
public class EstadoPrestamo
{
    [Key]
    [Column("estadoprestamoid")]
    public long EstadoPrestamoId { get; set; }

    [Column("estadoprestamonombre")]
    [Required]
    [MaxLength(40)]
    public string EstadoPrestamoNombre { get; set; } = string.Empty;

    [Column("estadoprestamodescripcion")]
    [MaxLength(200)]
    public string? EstadoPrestamoDescripcion { get; set; }

    [Column("estadoprestamoestaactivo")]
    public bool EstadoPrestamoEstaActivo { get; set; }

    // Navigation properties
    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
