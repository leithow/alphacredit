using AlphaCredit.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlphaCredit.Api.Controllers;

[ApiController]
[Route("api/prestamos/mora")]
public class PrestamosMoraController : ControllerBase
{
    private readonly PrestamoMoraCalculoService _moraCalculoService;

    public PrestamosMoraController(PrestamoMoraCalculoService moraCalculoService)
    {
        _moraCalculoService = moraCalculoService;
    }

    /// <summary>
    /// Ejecuta el cálculo diario de mora para todos los préstamos con cuotas vencidas
    /// Este endpoint debe ser llamado diariamente (automáticamente o manualmente)
    /// </summary>
    /// <remarks>
    /// - Genera componentes de MORA para cuotas vencidas
    /// - Usa la fecha del sistema (tabla fechasistema)
    /// - Solo genera mora del día actual, no acumulativa
    /// - Actualiza el saldo de mora de cada préstamo afectado
    /// </remarks>
    [HttpPost("calcular-diario")]
    public async Task<ActionResult<object>> CalcularMoraDiaria()
    {
        try
        {
            var inicio = DateTime.Now;

            var prestamosAfectados = await _moraCalculoService.GenerarComponentesMoraDiariaAsync();

            var fin = DateTime.Now;
            var duracion = (fin - inicio).TotalMilliseconds;

            return Ok(new
            {
                exitoso = true,
                prestamosAfectados,
                duracionMs = duracion,
                mensaje = $"Cálculo de mora completado. {prestamosAfectados} préstamo(s) procesado(s).",
                fechaEjecucion = fin
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                exitoso = false,
                mensaje = "Error al calcular mora diaria",
                detalle = ex.Message
            });
        }
    }

    /// <summary>
    /// Calcula la mora total actual de un préstamo específico
    /// (suma de todos los componentes MORA pendientes)
    /// </summary>
    [HttpGet("{prestamoId}/mora-total")]
    public async Task<ActionResult<object>> ObtenerMoraTotal(long prestamoId)
    {
        try
        {
            var moraTotal = await _moraCalculoService.CalcularMoraTotalPrestamoAsync(prestamoId);

            return Ok(new
            {
                prestamoId,
                moraTotal,
                mensaje = $"Mora total: {moraTotal:C}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                mensaje = "Error al calcular mora del préstamo",
                detalle = ex.Message
            });
        }
    }
}
