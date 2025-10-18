using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;
using AlphaCredit.Api.Services;
using AlphaCredit.Api.DTOs;

namespace AlphaCredit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrestamosController : ControllerBase
{
    private readonly AlphaCreditDbContext _context;
    private readonly ILogger<PrestamosController> _logger;
    private readonly PrestamoAmortizacionService _amortizacionService;

    public PrestamosController(AlphaCreditDbContext context, ILogger<PrestamosController> logger)
    {
        _context = context;
        _logger = logger;
        _amortizacionService = new PrestamoAmortizacionService();
    }

    // GET: api/prestamos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Prestamo>>> GetPrestamos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] long? personaId = null,
        [FromQuery] long? estadoId = null)
    {
        try
        {
            var query = _context.Prestamos
                .Include(p => p.Persona)
                .Include(p => p.EstadoPrestamo)
                .Include(p => p.FrecuenciaPago)
                .Include(p => p.DestinoCredito)
                .Include(p => p.Sucursal)
                .AsQueryable();

            // Filtros opcionales
            if (personaId.HasValue)
            {
                query = query.Where(p => p.PersonaId == personaId.Value);
            }

            if (estadoId.HasValue)
            {
                query = query.Where(p => p.EstadoPrestamoId == estadoId.Value);
            }

            var totalRecords = await query.CountAsync();

            var prestamos = await query
                .OrderByDescending(p => p.PrestamoFechaDesembolso)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            Response.Headers.Add("X-Page-Number", pageNumber.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(prestamos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los préstamos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/prestamos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Prestamo>> GetPrestamo(long id)
    {
        try
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Persona)
                .Include(p => p.EstadoPrestamo)
                .Include(p => p.FrecuenciaPago)
                .Include(p => p.DestinoCredito)
                .Include(p => p.Sucursal)
                .Include(p => p.PrestamoComponentes)
                .Include(p => p.PrestamoGarantias)
                    .ThenInclude(pg => pg.Garantia)
                .FirstOrDefaultAsync(p => p.PrestamoId == id);

            if (prestamo == null)
            {
                return NotFound(new { message = $"Préstamo con ID {id} no encontrado" });
            }

            return Ok(prestamo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el préstamo {PrestamoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/prestamos
    [HttpPost]
    public async Task<ActionResult<Prestamo>> CreatePrestamo([FromBody] PrestamoCreateRequest request)
    {
        try
        {
            // Validar que la persona existe
            var persona = await _context.Personas.FindAsync(request.PersonaId);
            if (persona == null)
            {
                return BadRequest(new { message = "La persona especificada no existe" });
            }

            // Validar número de cuotas
            if (request.PrestamoPlazo <= 0)
            {
                return BadRequest(new { message = "El número de cuotas debe ser mayor a cero" });
            }

            // Obtener frecuencia de pago
            var frecuenciaPago = await _context.FrecuenciasPago.FindAsync(request.FrecuenciaPagoId);
            if (frecuenciaPago == null)
            {
                return BadRequest(new { message = "La frecuencia de pago especificada no existe" });
            }

            // Obtener componentes de Capital e Interés
            var componenteCapital = await _context.ComponentesPrestamo
                .FirstOrDefaultAsync(c => c.ComponentePrestamoTipo == "CAPITAL" && c.ComponentePrestamoEstaActivo);

            var componenteInteres = await _context.ComponentesPrestamo
                .FirstOrDefaultAsync(c => c.ComponentePrestamoTipo == "INTERES" && c.ComponentePrestamoEstaActivo);

            if (componenteCapital == null || componenteInteres == null)
            {
                return BadRequest(new { message = "No se encontraron los componentes de Capital e Interés. Por favor, configure los catálogos." });
            }

            // Obtener estado componente vigente/pendiente (VIG)
            var estadoPendiente = await _context.EstadosComponente
                .FirstOrDefaultAsync(e => (e.EstadoComponenteNombre == "VIG" || e.EstadoComponenteNombre.Contains("Vigente")) && e.EstadoComponenteEstaActivo);

            if (estadoPendiente == null)
            {
                return BadRequest(new { message = "No se encontró el estado de componente 'Vigente' (VIG). Por favor, configure los catálogos." });
            }

            // Obtener estado del préstamo (por defecto buscar "Activo" o "Aprobado")
            long estadoPrestamoId;
            if (request.EstadoPrestamoId.HasValue && request.EstadoPrestamoId.Value > 0)
            {
                estadoPrestamoId = request.EstadoPrestamoId.Value;
            }
            else
            {
                // Buscar un estado activo por defecto
                var estadoDefault = await _context.EstadosPrestamo
                    .Where(e => e.EstadoPrestamoEstaActivo &&
                                (e.EstadoPrestamoNombre.ToUpper().Contains("ACTIVO") ||
                                 e.EstadoPrestamoNombre.ToUpper().Contains("APROBADO") ||
                                 e.EstadoPrestamoNombre.ToUpper().Contains("VIGENTE")))
                    .FirstOrDefaultAsync();

                if (estadoDefault == null)
                {
                    // Si no hay ninguno, tomar el primero activo
                    estadoDefault = await _context.EstadosPrestamo
                        .Where(e => e.EstadoPrestamoEstaActivo)
                        .FirstOrDefaultAsync();
                }

                if (estadoDefault == null)
                {
                    return BadRequest(new { message = "No se encontró ningún estado de préstamo activo. Por favor, configure los catálogos." });
                }

                estadoPrestamoId = estadoDefault.EstadoPrestamoId;
            }

            // Validar fondo y saldo disponible
            var fondo = await _context.Fondos.FindAsync(request.FondoId);
            if (fondo == null)
            {
                return BadRequest(new { message = "El fondo especificado no existe" });
            }

            if (!fondo.FondoEstaActivo)
            {
                return BadRequest(new { message = "El fondo seleccionado no está activo" });
            }

            if (fondo.FondoSaldoActual < request.PrestamoMonto)
            {
                return BadRequest(new
                {
                    message = $"El fondo '{fondo.FondoNombre}' no tiene saldo suficiente. Saldo disponible: {fondo.FondoSaldoActual:C}, Monto solicitado: {request.PrestamoMonto:C}",
                    saldoDisponible = fondo.FondoSaldoActual,
                    montoSolicitado = request.PrestamoMonto,
                    diferencia = request.PrestamoMonto - fondo.FondoSaldoActual
                });
            }

            // Validar que la moneda del préstamo coincida con la moneda del fondo
            if (fondo.MonedaId != request.MonedaId)
            {
                return BadRequest(new { message = "La moneda del préstamo debe coincidir con la moneda del fondo seleccionado" });
            }

            // Crear el préstamo
            var prestamo = new Prestamo
            {
                PersonaId = request.PersonaId,
                SucursalId = request.SucursalId,
                MonedaId = request.MonedaId,
                EstadoPrestamoId = estadoPrestamoId,
                FrecuenciaPagoId = request.FrecuenciaPagoId,
                DestinoCreditoId = request.DestinoCreditoId,
                FondoId = request.FondoId,
                PrestamoMonto = request.PrestamoMonto,
                PrestamoTasaInteres = request.PrestamoTasaInteres,
                PrestamoPlazo = request.PrestamoPlazo,
                PrestamoFechaAprobacion = request.PrestamoFechaAprobacion ?? DateTime.UtcNow,
                PrestamoFechaDesembolso = request.PrestamoFechaDesembolso ?? DateTime.UtcNow,
                PrestamoObservaciones = request.PrestamoObservaciones,
                PrestamoUserCrea = request.PrestamoUserCrea ?? "system",
                PrestamoFechaCreacion = DateTime.UtcNow
            };

            // Generar número de préstamo si no existe
            if (string.IsNullOrEmpty(prestamo.PrestamoNumero))
            {
                var ultimoNumero = await _context.Prestamos
                    .OrderByDescending(p => p.PrestamoId)
                    .Select(p => p.PrestamoId)
                    .FirstOrDefaultAsync();

                prestamo.PrestamoNumero = $"PRES-{DateTime.Now.Year}-{(ultimoNumero + 1):D6}";
            }

            // Calcular fecha de vencimiento
            prestamo.PrestamoFechaVencimiento = prestamo.PrestamoFechaDesembolso?
                .AddDays(frecuenciaPago.FrecuenciaPagoDias * request.PrestamoPlazo);

            // Guardar el préstamo primero para obtener el ID
            _context.Prestamos.Add(prestamo);
            await _context.SaveChangesAsync();

            // Generar componentes (Capital e Interés) automáticamente
            var componentes = _amortizacionService.GenerarComponentes(
                prestamoId: prestamo.PrestamoId,
                montoPrestamo: request.PrestamoMonto,
                tasaInteresAnual: request.PrestamoTasaInteres,
                numeroCuotas: request.PrestamoPlazo,
                frecuenciaPagoDias: frecuenciaPago.FrecuenciaPagoDias,
                fechaDesembolso: prestamo.PrestamoFechaDesembolso ?? DateTime.UtcNow,
                esAlVencimiento: request.EsAlVencimiento,
                componenteCapitalId: componenteCapital.ComponentePrestamoId,
                componenteInteresId: componenteInteres.ComponentePrestamoId,
                estadoComponentePendienteId: estadoPendiente.EstadoComponenteId
            );

            // Guardar los componentes
            await _context.PrestamosComponentes.AddRangeAsync(componentes);

            // Calcular saldos iniciales
            prestamo.PrestamoSaldoCapital = componentes
                .Where(c => c.ComponentePrestamoId == componenteCapital.ComponentePrestamoId)
                .Sum(c => c.PrestamoComponenteMonto);

            prestamo.PrestamoSaldoInteres = componentes
                .Where(c => c.ComponentePrestamoId == componenteInteres.ComponentePrestamoId)
                .Sum(c => c.PrestamoComponenteMonto);

            prestamo.PrestamoSaldoMora = 0;

            // Crear movimiento de fondo (EGRESO) por el desembolso del préstamo
            var fondoMovimiento = new FondoMovimiento
            {
                FondoId = request.FondoId,
                FondoMovimientoTipo = "EGRESO",
                FondoMovimientoMonto = request.PrestamoMonto,
                FondoMovimientoConcepto = $"Desembolso de préstamo {prestamo.PrestamoNumero} a {persona.PersonaNombreCompleto} - REF: PRESTAMO-{prestamo.PrestamoId}",
                FondoMovimientoFecha = prestamo.PrestamoFechaDesembolso ?? DateTime.UtcNow,
                FondoMovimientoUserCrea = request.PrestamoUserCrea ?? "system",
                FondoMovimientoFechaCreacion = DateTime.UtcNow
            };

            _context.FondosMovimientos.Add(fondoMovimiento);

            // Actualizar saldo del fondo
            fondo.FondoSaldoActual -= request.PrestamoMonto;
            fondo.FondoFechaModifica = DateTime.UtcNow;
            fondo.FondoUserModifica = request.PrestamoUserCrea ?? "system";

            await _context.SaveChangesAsync();

            // Cargar el préstamo con sus relaciones
            var prestamoCreado = await _context.Prestamos
                .Include(p => p.Persona)
                .Include(p => p.EstadoPrestamo)
                .Include(p => p.FrecuenciaPago)
                .Include(p => p.PrestamoComponentes)
                    .ThenInclude(pc => pc.ComponentePrestamo)
                .FirstOrDefaultAsync(p => p.PrestamoId == prestamo.PrestamoId);

            return CreatedAtAction(nameof(GetPrestamo), new { id = prestamo.PrestamoId }, prestamoCreado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el préstamo");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    // PUT: api/prestamos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePrestamo(long id, Prestamo prestamo)
    {
        if (id != prestamo.PrestamoId)
        {
            return BadRequest(new { message = "El ID del préstamo no coincide" });
        }

        try
        {
            var existingPrestamo = await _context.Prestamos.FindAsync(id);
            if (existingPrestamo == null)
            {
                return NotFound(new { message = $"Préstamo con ID {id} no encontrado" });
            }

            // Actualizar solo campos permitidos
            existingPrestamo.EstadoPrestamoId = prestamo.EstadoPrestamoId;
            existingPrestamo.PrestamoTasaInteres = prestamo.PrestamoTasaInteres;
            existingPrestamo.PrestamoPlazo = prestamo.PrestamoPlazo;
            existingPrestamo.FrecuenciaPagoId = prestamo.FrecuenciaPagoId;
            existingPrestamo.DestinoCreditoId = prestamo.DestinoCreditoId;
            existingPrestamo.PrestamoObservaciones = prestamo.PrestamoObservaciones;
            existingPrestamo.PrestamoFechaModifica = DateTime.UtcNow;
            existingPrestamo.PrestamoUserModifica = prestamo.PrestamoUserModifica;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await PrestamoExists(id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el préstamo {PrestamoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/prestamos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePrestamo(long id)
    {
        try
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.PrestamoComponentes)
                .Include(p => p.PrestamoGarantias)
                .FirstOrDefaultAsync(p => p.PrestamoId == id);

            if (prestamo == null)
            {
                return NotFound(new { message = $"Préstamo con ID {id} no encontrado" });
            }

            // Verificar si tiene pagos realizados
            var tienePagos = await _context.MovimientosPrestamo
                .AnyAsync(m => m.PrestamoId == id);

            if (tienePagos)
            {
                return BadRequest(new { message = "No se puede eliminar un préstamo que ya tiene pagos realizados" });
            }

            // Eliminar componentes y garantías relacionadas
            _context.PrestamosComponentes.RemoveRange(prestamo.PrestamoComponentes);
            _context.PrestamosGarantias.RemoveRange(prestamo.PrestamoGarantias);

            _context.Prestamos.Remove(prestamo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el préstamo {PrestamoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/prestamos/persona/5
    [HttpGet("persona/{personaId}")]
    public async Task<ActionResult<IEnumerable<Prestamo>>> GetPrestamosByPersona(long personaId)
    {
        try
        {
            var prestamos = await _context.Prestamos
                .Where(p => p.PersonaId == personaId)
                .Include(p => p.EstadoPrestamo)
                .Include(p => p.FrecuenciaPago)
                .OrderByDescending(p => p.PrestamoFechaDesembolso)
                .ToListAsync();

            return Ok(prestamos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los préstamos de la persona {PersonaId}", personaId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/prestamos/activos
    [HttpGet("activos")]
    public async Task<ActionResult<IEnumerable<Prestamo>>> GetPrestamosActivos()
    {
        try
        {
            var estadoActivo = await _context.EstadosPrestamo
                .Where(e => e.EstadoPrestamoEstaActivo && e.EstadoPrestamoNombre.Contains("Activo"))
                .Select(e => e.EstadoPrestamoId)
                .FirstOrDefaultAsync();

            var prestamos = await _context.Prestamos
                .Where(p => p.EstadoPrestamoId == estadoActivo && p.PrestamoSaldoCapital > 0)
                .Include(p => p.Persona)
                .Include(p => p.EstadoPrestamo)
                .OrderByDescending(p => p.PrestamoFechaDesembolso)
                .ToListAsync();

            return Ok(prestamos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los préstamos activos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/prestamos/5/amortizacion
    [HttpGet("{id}/amortizacion")]
    public async Task<ActionResult<IEnumerable<CuotaAmortizacion>>> GetTablaAmortizacion(long id)
    {
        try
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.FrecuenciaPago)
                .FirstOrDefaultAsync(p => p.PrestamoId == id);

            if (prestamo == null)
            {
                return NotFound(new { message = $"Préstamo con ID {id} no encontrado" });
            }

            // Determinar si es al vencimiento basado en el número de componentes
            var numeroComponentes = await _context.PrestamosComponentes
                .Where(pc => pc.PrestamoId == id)
                .CountAsync();

            var esAlVencimiento = numeroComponentes == 2; // 1 capital + 1 interés = al vencimiento

            var tablaAmortizacion = _amortizacionService.GenerarTablaAmortizacion(
                montoPrestamo: prestamo.PrestamoMonto,
                tasaInteresAnual: prestamo.PrestamoTasaInteres,
                numeroCuotas: prestamo.PrestamoPlazo,
                frecuenciaPagoDias: prestamo.FrecuenciaPago.FrecuenciaPagoDias,
                fechaDesembolso: prestamo.PrestamoFechaDesembolso ?? DateTime.UtcNow,
                esAlVencimiento: esAlVencimiento
            );

            return Ok(tablaAmortizacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar la tabla de amortización del préstamo {PrestamoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/prestamos/calcular-amortizacion
    [HttpPost("calcular-amortizacion")]
    public ActionResult<IEnumerable<CuotaAmortizacion>> CalcularAmortizacion([FromBody] CalcularAmortizacionRequest request)
    {
        try
        {
            if (request.Monto <= 0 || request.TasaInteres <= 0 || request.Plazo <= 0 || request.FrecuenciaPagoDias <= 0)
            {
                return BadRequest(new { message = "Todos los valores deben ser mayores a cero" });
            }

            var tablaAmortizacion = _amortizacionService.GenerarTablaAmortizacion(
                montoPrestamo: request.Monto,
                tasaInteresAnual: request.TasaInteres,
                numeroCuotas: request.Plazo,
                frecuenciaPagoDias: (short)request.FrecuenciaPagoDias,
                fechaDesembolso: DateTime.UtcNow,
                esAlVencimiento: false // Siempre amortización francesa para preview
            );

            return Ok(tablaAmortizacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular la amortización");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    private async Task<bool> PrestamoExists(long id)
    {
        return await _context.Prestamos.AnyAsync(e => e.PrestamoId == id);
    }
}

// DTO para calcular amortización
public class CalcularAmortizacionRequest
{
    public decimal Monto { get; set; }
    public decimal TasaInteres { get; set; }
    public int Plazo { get; set; }
    public int FrecuenciaPagoDias { get; set; }
}
