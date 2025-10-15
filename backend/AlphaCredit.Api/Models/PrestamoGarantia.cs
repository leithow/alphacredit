using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("prestamogarantia")]
public class PrestamoGarantia
{
    [Key]
    [Column("prestamogarantiaid")]
    public long PrestamoGarantiaId { get; set; }

    [Column("prestamoid")]
    public long PrestamoId { get; set; }

    [Column("garantiaid")]
    public long GarantiaId { get; set; }

    [Column("prestamogarantiafechaasignacion")]
    public DateTime PrestamoGarantiaFechaAsignacion { get; set; }

    [Column("prestamogarantiaestaactiva")]
    public bool PrestamoGarantiaEstaActiva { get; set; }

    // Navigation properties
    [ForeignKey("PrestamoId")]
    public virtual Prestamo Prestamo { get; set; } = null!;

    [ForeignKey("GarantiaId")]
    public virtual Garantia Garantia { get; set; } = null!;
}
