using System.ComponentModel.DataAnnotations;

namespace AlphaCredit.Api.DTOs;

public class SucursalCreateRequest
{
    [Required]
    public long EmpresaId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SucursalNombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? SucursalDireccion { get; set; }

    [MaxLength(20)]
    public string? SucursalTelefono { get; set; }
}
