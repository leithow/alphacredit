using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("sucursal")]
public class Sucursal
{
    [Key]
    [Column("sucursalid")]
    public long SucursalId { get; set; }

    [Column("empresaid")]
    public long EmpresaId { get; set; }

    [Column("sucursalnombre")]
    [Required]
    [MaxLength(100)]
    public string SucursalNombre { get; set; } = string.Empty;

    [Column("sucursaldireccion")]
    [MaxLength(500)]
    public string? SucursalDireccion { get; set; }

    [Column("sucursaltelefono")]
    [MaxLength(20)]
    public string? SucursalTelefono { get; set; }

    [Column("sucursalestaactiva")]
    public bool SucursalEstaActiva { get; set; }

    // Navigation properties
    [ForeignKey("EmpresaId")]
    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
