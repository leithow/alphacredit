using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("prestamo_documento_impresion")]
public class PrestamoDocumentoImpresion
{
    [Key]
    [Column("prestamo_documento_impresionid")]
    public long PrestamoDocumentoImpresionId { get; set; }

    [Column("prestamo_documentoid")]
    public long PrestamoDocumentoId { get; set; }

    [Column("prestamo_documento_impresion_fecha")]
    public DateTime PrestamoDocumentoImpresionFecha { get; set; } = DateTime.UtcNow;

    [Column("prestamo_documento_impresion_usuario")]
    [MaxLength(40)]
    public string? PrestamoDocumentoImpresionUsuario { get; set; }

    [Column("prestamo_documento_impresion_ip")]
    [MaxLength(50)]
    public string? PrestamoDocumentoImpresionIp { get; set; }

    [Column("prestamo_documento_impresion_observaciones")]
    [MaxLength(500)]
    public string? PrestamoDocumentoImpresionObservaciones { get; set; }

    // Navigation properties
    [ForeignKey("PrestamoDocumentoId")]
    public virtual PrestamoDocumento PrestamoDocumento { get; set; } = null!;
}
