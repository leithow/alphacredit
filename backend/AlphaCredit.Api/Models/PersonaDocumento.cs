using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("personadocumento")]
public class PersonaDocumento
{
    [Key]
    [Column("personadocumentoid")]
    public long PersonaDocumentoId { get; set; }

    [Column("personaid")]
    public long PersonaId { get; set; }

    [Column("personadocumentopath")]
    [Required, MaxLength(1024)]
    public string PersonaDocumentoPath { get; set; } = string.Empty;

    [Column("personadocumentotipo")]
    [MaxLength(20)]
    public string? PersonaDocumentoTipo { get; set; }

    [Column("personadocumentofechacreacion")]
    public DateTime PersonaDocumentoFechaCreacion { get; set; }

    [Column("personadocumentofechamodifica")]
    public DateTime? PersonaDocumentoFechaModifica { get; set; }

    [ForeignKey("PersonaId")]
    public virtual Persona Persona { get; set; } = null!;
}
