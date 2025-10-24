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
    /// Obtiene la fecha actual del sistema basada en la tabla fechasistema
    /// Si el último registro está cerrado (estacerrado = true), retorna el día siguiente
    /// Si está abierto (estacerrado = false), retorna esa fecha
    /// </summary>
    public async Task<DateTime> ObtenerFechaActualAsync()
    {
        var ultimaFecha = await _context.FechasSistema
            .OrderByDescending(f => f.FechaSistemaFecha)
            .FirstOrDefaultAsync();

        DateTime fecha;
        if (ultimaFecha == null)
        {
            // Si no hay registros, usar la fecha actual del servidor (solo fecha, sin hora)
            fecha = DateTime.UtcNow.Date;
        }
        else if (ultimaFecha.FechaSistemaEstaCerrado)
        {
            // Si el día está cerrado, la fecha actual es el día siguiente
            fecha = ultimaFecha.FechaSistemaFecha.AddDays(1).Date;
        }
        else
        {
            // Si el día está abierto, usar esa fecha
            fecha = ultimaFecha.FechaSistemaFecha.Date;
        }

        // Asegurar que la fecha tenga Kind=Utc para PostgreSQL
        return DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
    }

    /// <summary>
    /// Obtiene solo la fecha (sin hora) del sistema
    /// </summary>
    public async Task<DateTime> ObtenerFechaActualSoloFechaAsync()
    {
        return await ObtenerFechaActualAsync();
    }

    /// <summary>
    /// Obtiene la fecha actual del sistema de forma síncrona (para contextos no-async)
    /// </summary>
    public DateTime ObtenerFechaActual()
    {
        var ultimaFecha = _context.FechasSistema
            .OrderByDescending(f => f.FechaSistemaFecha)
            .FirstOrDefault();

        DateTime fecha;
        if (ultimaFecha == null)
        {
            fecha = DateTime.UtcNow.Date;
        }
        else if (ultimaFecha.FechaSistemaEstaCerrado)
        {
            fecha = ultimaFecha.FechaSistemaFecha.AddDays(1).Date;
        }
        else
        {
            fecha = ultimaFecha.FechaSistemaFecha.Date;
        }

        // Asegurar que la fecha tenga Kind=Utc para PostgreSQL
        return DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
    }
}
