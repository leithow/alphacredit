using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("prestamocomponente")]
public class PrestamoComponente
{
    [Key]
    [Column("prestamocomponenteid")]
    public long PrestamoComponenteId { get; set; }

    [Column("prestamoid")]
    public long PrestamoId { get; set; }

    [Column("componenteprestamoid")]
    public long ComponentePrestamoId { get; set; }

    [Column("estadocomponenteid")]
    public long EstadoComponenteId { get; set; }

    [Column("prestamocomponentemonto", TypeName = "decimal(18,2)")]
    public decimal PrestamoComponenteMonto { get; set; }

    [Column("prestamocomponentefechavencimiento")]
    public DateTime? PrestamoComponenteFechaVencimiento { get; set; }

    [Column("prestamocomponentesaldo", TypeName = "decimal(18,2)")]
    public decimal PrestamoComponenteSaldo { get; set; }

    [Column("prestamocomponentenumerocuota")]
    public int? PrestamoComponenteNumeroCuota { get; set; }

    // Navigation properties
    [ForeignKey("PrestamoId")]
    public virtual Prestamo Prestamo { get; set; } = null!;

    [ForeignKey("ComponentePrestamoId")]
    public virtual ComponentePrestamo ComponentePrestamo { get; set; } = null!;

    [ForeignKey("EstadoComponenteId")]
    public virtual EstadoComponente EstadoComponente { get; set; } = null!;
}
