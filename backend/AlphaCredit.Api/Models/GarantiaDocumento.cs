using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("garantiadocumento")]
public class GarantiaDocumento
{
    [Key]
    [Column("garantiadocumentoid")]
    public long GarantiaDocumentoId { get; set; }

    [Column("garantiaid")]
    public long GarantiaId { get; set; }

    [Column("garantiadocumentopath")]
    [Required, MaxLength(1024)]
    public string GarantiaDocumentoPath { get; set; } = string.Empty;

    [Column("garantiadocumentotipo")]
    [MaxLength(20)]
    public string? GarantiaDocumentoTipo { get; set; }

    [Column("garantiadocumentofechacreacion")]
    public DateTime GarantiaDocumentoFechaCreacion { get; set; }

    [Column("garantiadocumentofechamodifica")]
    public DateTime? GarantiaDocumentoFechaModifica { get; set; }

    [ForeignKey("GarantiaId")]
    public virtual Garantia Garantia { get; set; } = null!;
}