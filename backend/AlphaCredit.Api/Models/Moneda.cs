using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("moneda")]
public class Moneda
{
    [Key]
    [Column("monedaid")]
    public long MonedaId { get; set; }

    [Column("monedanombre")]
    [Required]
    [MaxLength(60)]
    public string MonedaNombre { get; set; } = string.Empty;

    [Column("monedacodigo")]
    [Required]
    [MaxLength(5)]
    public string MonedaCodigo { get; set; } = string.Empty;

    [Column("monedasimbolo")]
    [MaxLength(5)]
    public string? MonedaSimbolo { get; set; }

    [Column("monedaestaactiva")]
    public bool MonedaEstaActiva { get; set; }

    // Navigation properties
    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public virtual ICollection<Fondo> Fondos { get; set; } = new List<Fondo>();
}
