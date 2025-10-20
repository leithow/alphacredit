using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("prestamo_documento")]
public class PrestamoDocumento
{
    [Key]
    [Column("prestamo_documentoid")]
    public long PrestamoDocumentoId { get; set; }

    [Column("prestamoid")]
    public long PrestamoId { get; set; }

    [Column("prestamo_documento_tipo")]
    [Required]
    [MaxLength(50)]
    public string PrestamoDocumentoTipo { get; set; } = string.Empty; // CONTRATO, PAGARE, PLAN_PAGOS

    [Column("prestamo_documento_ruta")]
    [MaxLength(500)]
    public string? PrestamoDocumentoRuta { get; set; }

    [Column("prestamo_documento_hash")]
    [MaxLength(100)]
    public string? PrestamoDocumentoHash { get; set; } // Para verificar integridad

    [Column("prestamo_documento_veces_impreso")]
    public int PrestamoDocumentoVecesImpreso { get; set; } = 0;

    [Column("prestamo_documento_fecha_primera_impresion")]
    public DateTime? PrestamoDocumentoFechaPrimeraImpresion { get; set; }

    [Column("prestamo_documento_fecha_ultima_impresion")]
    public DateTime? PrestamoDocumentoFechaUltimaImpresion { get; set; }

    [Column("prestamo_documento_user_crea")]
    [MaxLength(40)]
    public string? PrestamoDocumentoUserCrea { get; set; }

    [Column("prestamo_documento_fecha_creacion")]
    public DateTime PrestamoDocumentoFechaCreacion { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("PrestamoId")]
    public virtual Prestamo Prestamo { get; set; } = null!;

    public virtual ICollection<PrestamoDocumentoImpresion> Impresiones { get; set; } = new List<PrestamoDocumentoImpresion>();
}
