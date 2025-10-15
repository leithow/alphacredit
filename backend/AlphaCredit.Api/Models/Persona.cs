using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlphaCredit.Api.Models;

[Table("persona")]
public class Persona
{
    [Key]
    [Column("personaid")]
    public long PersonaId { get; set; }

    [Column("personaprimernombre")]
    [Required]
    [MaxLength(40)]
    public string PersonaPrimerNombre { get; set; } = string.Empty;

    [Column("personasegundonombre")]
    [MaxLength(40)]
    public string? PersonaSegundoNombre { get; set; }

    [Column("personaprimerapellido")]
    [Required]
    [MaxLength(40)]
    public string PersonaPrimerApellido { get; set; } = string.Empty;

    [Column("personasegundoapellido")]
    [MaxLength(40)]
    public string? PersonaSegundoApellido { get; set; }

    [Column("tipoidentificacionid")]
    public long TipoIdentificacionId { get; set; }

    [Column("personaidentificacion")]
    [Required]
    [MaxLength(20)]
    public string PersonaIdentificacion { get; set; } = string.Empty;

    [Column("personafechanacimiento")]
    public DateTime PersonaFechaNacimiento { get; set; }

    [Column("personaesnatural")]
    public bool PersonaEsNatural { get; set; }

    [Column("personaesempleado")]
    public bool PersonaEsEmpleado { get; set; }

    [Column("personaescliente")]
    public bool PersonaEsCliente { get; set; }

    [Column("personaesproveedor")]
    public bool PersonaEsProveedor { get; set; }

    [Column("estadocivilid")]
    public long? EstadoCivilId { get; set; }

    [Column("personadireccion")]
    [Required]
    [MaxLength(1024)]
    public string PersonaDireccion { get; set; } = string.Empty;

    [Column("personageolocalizacion")]
    [MaxLength(50)]
    public string? PersonaGeolocalizacion { get; set; }

    [Column("personaemail")]
    [MaxLength(100)]
    public string? PersonaEmail { get; set; }

    [Column("personaestaactiva")]
    public bool PersonaEstaActiva { get; set; }

    [Column("personausercrea")]
    [MaxLength(40)]
    public string? PersonaUserCrea { get; set; }

    [Column("personausermodifica")]
    [MaxLength(40)]
    public string? PersonaUserModifica { get; set; }

    [Column("personafechacreacion")]
    public DateTime PersonaFechaCreacion { get; set; }

    [Column("personafechamodifica")]
    public DateTime? PersonaFechaModifica { get; set; }

    [Column("sexoid")]
    public long? SexoId { get; set; }

    [Column("personanombrecompleto")]
    [Required]
    [MaxLength(120)]
    public string PersonaNombreCompleto { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey("TipoIdentificacionId")]
    public virtual TipoIdentificacion? TipoIdentificacion { get; set; }

    [ForeignKey("EstadoCivilId")]
    public virtual EstadoCivil? EstadoCivil { get; set; }

    [ForeignKey("SexoId")]
    public virtual Sexo? Sexo { get; set; }

    public virtual ICollection<PersonaTelefono> PersonaTelefonos { get; set; } = new List<PersonaTelefono>();
    public virtual ICollection<PersonaActividad> PersonaActividades { get; set; } = new List<PersonaActividad>();
    public virtual ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public virtual ICollection<Garantia> Garantias { get; set; } = new List<Garantia>();
}
