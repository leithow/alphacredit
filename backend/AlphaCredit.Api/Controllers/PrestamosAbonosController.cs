using AlphaCredit.Api.DTOs;
using AlphaCredit.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlphaCredit.Api.Controllers;

[ApiController]
[Route("api/prestamos")]
public class PrestamosAbonosController : ControllerBase
{
    private readonly PrestamoAbonoService _abonoService;
    private readonly PrestamoEstadoCuentaService _estadoCuentaService;
    private readonly PrestamoMoraService _moraService;
    private readonly ReciboAbonoService _reciboService;

    public PrestamosAbonosController(
        PrestamoAbonoService abonoService,
        PrestamoEstadoCuentaService estadoCuentaService,
        PrestamoMoraService moraService,
        ReciboAbonoService reciboService)
    {
        _abonoService = abonoService;
        _estadoCuentaService = estadoCuentaService;
        _moraService = moraService;
        _reciboService = reciboService;
    }

    /// <summary>
    /// Aplica un abono a un préstamo
    /// </summary>
    /// <remarks>
    /// Tipos de abono soportados:
    /// - CUOTA: Paga cuotas completas en orden de vencimiento
    /// - PARCIAL: Abono parcial con distribución automática o manual
    /// - CAPITAL: Abono directo a capital
    /// - MORA: Pago específico de mora
    /// </remarks>
    [HttpPost("{prestamoId}/abonos")]
    public async Task<ActionResult<AbonoPrestamoResponse>> AplicarAbono(
        long prestamoId,
        [FromBody] AbonoPrestamoRequest request)
    {
        try
        {
            // Asignar el prestamoId de la URL al request
            request.PrestamoId = prestamoId;

            // Obtener usuario del contexto (ajustar según tu implementación de autenticación)
            var usuario = User?.Identity?.Name ?? "SISTEMA";

            var resultado = await _abonoService.AplicarAbonoAsync(request, usuario);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            var detalleCompleto = ex.InnerException != null
                ? $"{ex.Message} | Inner: {ex.InnerException.Message}"
                : ex.Message;

            return StatusCode(500, new { mensaje = "Error al aplicar el abono", detalle = detalleCompleto, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Obtiene el estado de cuenta completo de un préstamo
    /// </summary>
    [HttpGet("{prestamoId}/estado-cuenta")]
    public async Task<ActionResult<EstadoCuentaPrestamoDto>> ObtenerEstadoCuenta(long prestamoId)
    {
        try
        {
            var estadoCuenta = await _estadoCuentaService.ObtenerEstadoCuentaAsync(prestamoId);
            return Ok(estadoCuenta);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al obtener estado de cuenta", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el historial de pagos de un préstamo
    /// </summary>
    [HttpGet("{prestamoId}/historial-pagos")]
    public async Task<ActionResult<List<MovimientoPrestamoDto>>> ObtenerHistorialPagos(long prestamoId)
    {
        try
        {
            var historial = await _estadoCuentaService.ObtenerHistorialPagosAsync(prestamoId);
            return Ok(historial);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al obtener historial de pagos", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Calcula la mora de un préstamo a una fecha específica
    /// </summary>
    [HttpGet("{prestamoId}/calcular-mora")]
    public async Task<ActionResult<object>> CalcularMora(
        long prestamoId,
        [FromQuery] DateTime? fecha)
    {
        try
        {
            var fechaCalculo = fecha ?? DateTime.Now;
            var mora = await _moraService.CalcularMoraPrestamoAsync(prestamoId, fechaCalculo);

            return Ok(new
            {
                prestamoId,
                fechaCalculo,
                mora
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al calcular mora", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Simula un abono sin aplicarlo (para preview)
    /// </summary>
    [HttpPost("{prestamoId}/abonos/simular")]
    public async Task<ActionResult<object>> SimularAbono(
        long prestamoId,
        [FromBody] AbonoPrestamoRequest request)
    {
        try
        {
            // Esta funcionalidad podría implementarse creando una transacción
            // y haciendo rollback al final, o creando un servicio de simulación
            return Ok(new
            {
                mensaje = "Funcionalidad de simulación en desarrollo",
                monto = request.Monto,
                tipoAbono = request.TipoAbono
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al simular abono", detalle = ex.Message });
        }
    }

    /// <summary>
    /// Genera y descarga el recibo de un pago en PDF
    /// </summary>
    [HttpGet("abonos/{movimientoPrestamoId}/recibo")]
    public async Task<ActionResult> ObtenerReciboPdf(long movimientoPrestamoId)
    {
        try
        {
            var pdfBytes = await _reciboService.GenerarReciboPdfAsync(movimientoPrestamoId);

            return File(pdfBytes, "application/pdf", $"Recibo-Pago-{movimientoPrestamoId}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al generar recibo", detalle = ex.Message });
        }
    }
}
