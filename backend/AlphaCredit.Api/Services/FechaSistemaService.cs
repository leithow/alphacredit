using AlphaCredit.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AlphaCredit.Api.Services;

/// <summary>
/// Servicio para manejar la fecha del sistema
/// </summary>
public class FechaSistemaService
{
    private readonly AlphaCreditDbContext _context;

    public FechaSistemaService(AlphaCreditDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene la fecha actual del sistema (fecha activa en la tabla fechasistema)
    /// </summary>
    public async Task<DateTime> ObtenerFechaActualAsync()
    {
        var fechaSistema = await _context.FechasSistema
            .Where(f => f.FechaSistemaEstaActiva)
            .OrderByDescending(f => f.FechaSistemaFecha)
            .FirstOrDefaultAsync();

        if (fechaSistema == null)
        {
            // Si no hay fecha del sistema configurada, usar fecha actual del servidor
            return DateTime.Now;
        }

        return fechaSistema.FechaSistemaFecha;
    }

    /// <summary>
    /// Obtiene solo la fecha (sin hora) del sistema
    /// </summary>
    public async Task<DateTime> ObtenerFechaActualSoloFechaAsync()
    {
        var fecha = await ObtenerFechaActualAsync();
        return fecha.Date;
    }
}
