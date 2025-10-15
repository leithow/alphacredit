using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("estadocomponente")]
public class EstadoComponente
{
    [Key]
    [Column("estadocomponenteid")]
    public long EstadoComponenteId { get; set; }

    [Column("estadocomponentenombre")]
    [Required]
    [MaxLength(40)]
    public string EstadoComponenteNombre { get; set; } = string.Empty;

    [Column("estadocomponentedescripcion")]
    [MaxLength(200)]
    public string? EstadoComponenteDescripcion { get; set; }

    [Column("estadocomponenteestaactivo")]
    public bool EstadoComponenteEstaActivo { get; set; }

    // Navigation properties
    public virtual ICollection<PrestamoComponente> PrestamoComponentes { get; set; } = new List<PrestamoComponente>();
}
