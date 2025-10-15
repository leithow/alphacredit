using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("personatelefono")]
public class PersonaTelefono
{
    [Key]
    [Column("personatelefonoid")]
    public long PersonaTelefonoId { get; set; }

    [Column("personaid")]
    public long PersonaId { get; set; }

    [Column("operadortelefonoid")]
    public long OperadorTelefonoId { get; set; }

    [Column("personatelefonumero")]
    [Required]
    [MaxLength(20)]
    public string PersonaTelefoNumero { get; set; } = string.Empty;

    [Column("personatelefonoesprincipal")]
    public bool PersonaTelefonoEsPrincipal { get; set; }

    [Column("personatelefonoestaactivo")]
    public bool PersonaTelefonoEstaActivo { get; set; }

    // Navigation properties
    [ForeignKey("PersonaId")]
    public virtual Persona Persona { get; set; } = null!;

    [ForeignKey("OperadorTelefonoId")]
    public virtual OperadorTelefono OperadorTelefono { get; set; } = null!;
}
