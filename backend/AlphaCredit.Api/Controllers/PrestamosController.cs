using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;

namespace AlphaCredit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrestamosController : ControllerBase
{
    private readonly AlphaCreditDbContext _context;
    private readonly ILogger<PrestamosController> _logger;

    public PrestamosController(AlphaCreditDbContext context, ILogger<PrestamosController> logger)
    {
        _context = context;
        _logger = logger;
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
    public async Task<ActionResult<Prestamo>> CreatePrestamo(Prestamo prestamo)
    {
        try
        {
            // Validar que la persona existe
            var persona = await _context.Personas.FindAsync(prestamo.PersonaId);
            if (persona == null)
            {
                return BadRequest(new { message = "La persona especificada no existe" });
            }

            // Generar número de préstamo si no existe
            if (string.IsNullOrEmpty(prestamo.PrestamoNumero))
            {
                var ultimoNumero = await _context.Prestamos
                    .OrderByDescending(p => p.PrestamoId)
                    .Select(p => p.PrestamoId)
                    .FirstOrDefaultAsync();

                prestamo.PrestamoNumero = $"PRES-{DateTime.Now.Year}-{(ultimoNumero + 1):D6}";
            }

            // Inicializar saldos
            prestamo.PrestamoSaldoCapital = prestamo.PrestamoMonto;
            prestamo.PrestamoSaldoInteres = 0;
            prestamo.PrestamoSaldoMora = 0;
            prestamo.PrestamoFechaCreacion = DateTime.UtcNow;

            _context.Prestamos.Add(prestamo);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPrestamo), new { id = prestamo.PrestamoId }, prestamo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el préstamo");
            return StatusCode(500, "Error interno del servidor");
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

    private async Task<bool> PrestamoExists(long id)
    {
        return await _context.Prestamos.AnyAsync(e => e.PrestamoId == id);
    }
}
