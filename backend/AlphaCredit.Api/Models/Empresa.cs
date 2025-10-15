using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("empresa")]
public class Empresa
{
    [Key]
    [Column("empresaid")]
    public long EmpresaId { get; set; }

    [Column("empresanombre")]
    [Required]
    [MaxLength(200)]
    public string EmpresaNombre { get; set; } = string.Empty;

    [Column("empresartn")]
    [MaxLength(20)]
    public string? EmpresaRtn { get; set; }

    [Column("empresadireccion")]
    [MaxLength(500)]
    public string? EmpresaDireccion { get; set; }

    [Column("empresatelefono")]
    [MaxLength(20)]
    public string? EmpresaTelefono { get; set; }

    [Column("empresaemail")]
    [MaxLength(100)]
    public string? EmpresaEmail { get; set; }

    [Column("empresaestaactiva")]
    public bool EmpresaEstaActiva { get; set; }

    // Navigation properties
    public virtual ICollection<Sucursal> Sucursales { get; set; } = new List<Sucursal>();
}
