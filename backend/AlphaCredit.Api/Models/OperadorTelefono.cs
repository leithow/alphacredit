using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("operadortelefono")]
public class OperadorTelefono
{
    [Key]
    [Column("operadortelefonoid")]
    public long OperadorTelefonoId { get; set; }

    [Column("operadortelefonombre")]
    [Required]
    [MaxLength(60)]
    public string OperadorTelefonoNombre { get; set; } = string.Empty;

    [Column("operadortelefonoestaactivo")]
    public bool OperadorTelefonoEstaActivo { get; set; }

    // Navigation properties
    public virtual ICollection<PersonaTelefono> PersonaTelefonos { get; set; } = new List<PersonaTelefono>();
}
