using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;
using AlphaCredit.Api.DTOs;

namespace AlphaCredit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GarantiasController : ControllerBase
{
    private readonly AlphaCreditDbContext _context;
    private readonly ILogger<GarantiasController> _logger;

    public GarantiasController(AlphaCreditDbContext context, ILogger<GarantiasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/garantias
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Garantia>>> GetGarantias(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] long? personaId = null,
        [FromQuery] long? tipoGarantiaId = null,
        [FromQuery] long? estadoGarantiaId = null)
    {
        try
        {
            var query = _context.Garantias
                .Include(g => g.Persona)
                .Include(g => g.PersonaFiador)
                .Include(g => g.TipoGarantia)
                .Include(g => g.EstadoGarantia)
                .AsQueryable();

            // Filtros opcionales
            if (personaId.HasValue)
            {
                query = query.Where(g => g.PersonaId == personaId.Value);
            }

            if (tipoGarantiaId.HasValue)
            {
                query = query.Where(g => g.TipoGarantiaId == tipoGarantiaId.Value);
            }

            if (estadoGarantiaId.HasValue)
            {
                query = query.Where(g => g.EstadoGarantiaId == estadoGarantiaId.Value);
            }

            var totalRecords = await query.CountAsync();

            var garantias = await query
                .OrderByDescending(g => g.GarantiaFechaCreacion)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            Response.Headers.Add("X-Page-Number", pageNumber.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(garantias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las garantías");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/garantias/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Garantia>> GetGarantia(long id)
    {
        try
        {
            var garantia = await _context.Garantias
                .Include(g => g.Persona)
                .Include(g => g.PersonaFiador)
                .Include(g => g.TipoGarantia)
                .Include(g => g.EstadoGarantia)
                .Include(g => g.PrestamoGarantias)
                    .ThenInclude(pg => pg.Prestamo)
                .FirstOrDefaultAsync(g => g.GarantiaId == id);

            if (garantia == null)
            {
                return NotFound(new { message = $"Garantía con ID {id} no encontrada" });
            }

            return Ok(garantia);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la garantía {GarantiaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/garantias
    [HttpPost]
    public async Task<ActionResult<Garantia>> CreateGarantia(CreateGarantiaDto dto)
    {
        try
        {
            _logger.LogInformation("=== CREAR GARANTÍA ===");
            _logger.LogInformation($"PersonaId: {dto.PersonaId}");
            _logger.LogInformation($"TipoGarantiaId: {dto.TipoGarantiaId}");
            _logger.LogInformation($"EstadoGarantiaId: {dto.EstadoGarantiaId}");
            _logger.LogInformation($"PersonaFiadorId: {dto.PersonaFiadorId}");
            _logger.LogInformation($"Descripción: {dto.GarantiaDescripcion}");
            _logger.LogInformation($"ValorComercial: {dto.GarantiaValorComercial}");
            _logger.LogInformation($"ValorRealizable: {dto.GarantiaValorRealizable}");

            // Validar que la persona existe
            var persona = await _context.Personas.FindAsync(dto.PersonaId);
            if (persona == null)
            {
                _logger.LogError($"Persona con ID {dto.PersonaId} no existe");
                return BadRequest(new { message = "La persona especificada no existe" });
            }

            // Validar que el tipo de garantía existe
            var tipoGarantia = await _context.TiposGarantia.FindAsync(dto.TipoGarantiaId);
            if (tipoGarantia == null)
            {
                return BadRequest(new { message = "El tipo de garantía especificado no existe" });
            }

            // Validar que el estado de garantía existe
            var estadoGarantia = await _context.EstadosGarantia.FindAsync(dto.EstadoGarantiaId);
            if (estadoGarantia == null)
            {
                return BadRequest(new { message = "El estado de garantía especificado no existe" });
            }

            // Si es garantía fiduciaria, validar PersonaFiadorId
            if (tipoGarantia.TipoGarantiaNombre.ToUpper().Contains("FIDUCIARIA"))
            {
                if (!dto.PersonaFiadorId.HasValue)
                {
                    return BadRequest(new { message = "Para garantías fiduciarias debe especificar una persona fiadora" });
                }

                // Validar que la persona fiadora existe
                var personaFiador = await _context.Personas.FindAsync(dto.PersonaFiadorId.Value);
                if (personaFiador == null)
                {
                    return BadRequest(new { message = "La persona fiadora especificada no existe" });
                }

                // Esta validación se hará cuando se asigne la garantía a un préstamo
                // por ahora solo validamos que el fiador no sea la misma persona que el propietario
                if (dto.PersonaFiadorId == dto.PersonaId)
                {
                    return BadRequest(new { message = "El fiador no puede ser la misma persona que el propietario de la garantía" });
                }
            }

            // Crear la entidad Garantia desde el DTO
            var garantia = new Garantia
            {
                PersonaId = dto.PersonaId,
                TipoGarantiaId = dto.TipoGarantiaId,
                EstadoGarantiaId = dto.EstadoGarantiaId,
                PersonaFiadorId = dto.PersonaFiadorId,
                GarantiaDescripcion = dto.GarantiaDescripcion,
                GarantiaValorComercial = dto.GarantiaValorComercial,
                GarantiaValorRealizable = dto.GarantiaValorRealizable,
                GarantiaObservaciones = dto.GarantiaObservaciones,
                GarantiaUserCrea = dto.GarantiaUserCrea,
                GarantiaFechaCreacion = DateTime.UtcNow
            };

            _context.Garantias.Add(garantia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGarantia), new { id = garantia.GarantiaId }, garantia);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la garantía");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/garantias/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGarantia(long id, Garantia garantia)
    {
        if (id != garantia.GarantiaId)
        {
            return BadRequest(new { message = "El ID de la garantía no coincide" });
        }

        try
        {
            var existingGarantia = await _context.Garantias.FindAsync(id);
            if (existingGarantia == null)
            {
                return NotFound(new { message = $"Garantía con ID {id} no encontrada" });
            }

            // Validar tipo de garantía si está cambiando
            var tipoGarantia = await _context.TiposGarantia.FindAsync(garantia.TipoGarantiaId);
            if (tipoGarantia == null)
            {
                return BadRequest(new { message = "El tipo de garantía especificado no existe" });
            }

            // Si es garantía fiduciaria, validar PersonaFiadorId
            if (tipoGarantia.TipoGarantiaNombre.ToUpper().Contains("FIDUCIARIA"))
            {
                if (!garantia.PersonaFiadorId.HasValue)
                {
                    return BadRequest(new { message = "Para garantías fiduciarias debe especificar una persona fiadora" });
                }

                // Validar que la persona fiadora existe
                var personaFiador = await _context.Personas.FindAsync(garantia.PersonaFiadorId.Value);
                if (personaFiador == null)
                {
                    return BadRequest(new { message = "La persona fiadora especificada no existe" });
                }

                // Validar que el fiador no sea la misma persona que el propietario
                if (garantia.PersonaFiadorId == existingGarantia.PersonaId)
                {
                    return BadRequest(new { message = "El fiador no puede ser la misma persona que el propietario de la garantía" });
                }
            }

            // Actualizar campos
            existingGarantia.TipoGarantiaId = garantia.TipoGarantiaId;
            existingGarantia.EstadoGarantiaId = garantia.EstadoGarantiaId;
            existingGarantia.PersonaFiadorId = garantia.PersonaFiadorId;
            existingGarantia.GarantiaDescripcion = garantia.GarantiaDescripcion;
            existingGarantia.GarantiaValorComercial = garantia.GarantiaValorComercial;
            existingGarantia.GarantiaValorRealizable = garantia.GarantiaValorRealizable;
            existingGarantia.GarantiaObservaciones = garantia.GarantiaObservaciones;
            existingGarantia.GarantiaFechaModifica = DateTime.UtcNow;
            existingGarantia.GarantiaUserModifica = garantia.GarantiaUserModifica;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await GarantiaExists(id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la garantía {GarantiaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/garantias/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGarantia(long id)
    {
        try
        {
            var garantia = await _context.Garantias
                .Include(g => g.PrestamoGarantias)
                .FirstOrDefaultAsync(g => g.GarantiaId == id);

            if (garantia == null)
            {
                return NotFound(new { message = $"Garantía con ID {id} no encontrada" });
            }

            // Verificar si está asociada a préstamos
            if (garantia.PrestamoGarantias.Any())
            {
                return BadRequest(new { message = "No se puede eliminar una garantía que está asociada a préstamos" });
            }

            _context.Garantias.Remove(garantia);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la garantía {GarantiaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/garantias/prestamo/5
    [HttpGet("prestamo/{prestamoId}")]
    public async Task<ActionResult> GetGarantiasByPrestamo(
        long prestamoId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.PrestamosGarantias
                .Where(pg => pg.PrestamoId == prestamoId)
                .Include(pg => pg.Garantia)
                    .ThenInclude(g => g.TipoGarantia)
                .Include(pg => pg.Garantia)
                    .ThenInclude(g => g.EstadoGarantia)
                .Include(pg => pg.Garantia)
                    .ThenInclude(g => g.PersonaFiador)
                .Select(pg => pg.Garantia);

            var totalRecords = await query.CountAsync();

            var garantias = await query
                .OrderByDescending(g => g.GarantiaFechaCreacion)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            Response.Headers.Add("X-Page-Number", pageNumber.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            var response = new
            {
                data = garantias,
                totalCount = totalRecords,
                pageNumber,
                pageSize
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las garantías del préstamo {PrestamoId}", prestamoId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/garantias/persona/5
    [HttpGet("persona/{personaId}")]
    public async Task<ActionResult<IEnumerable<Garantia>>> GetGarantiasByPersona(
        long personaId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Garantias
                .Where(g => g.PersonaId == personaId)
                .Include(g => g.PersonaFiador)
                .Include(g => g.TipoGarantia)
                .Include(g => g.EstadoGarantia)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var garantias = await query
                .OrderByDescending(g => g.GarantiaFechaCreacion)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            Response.Headers.Add("X-Page-Number", pageNumber.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(garantias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las garantías de la persona {PersonaId}", personaId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/garantias/disponibles
    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<GarantiaDisponibleDto>>> GetGarantiasDisponibles(
        [FromQuery] long? personaId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Garantias
                .Include(g => g.Persona)
                .Include(g => g.PersonaFiador)
                .Include(g => g.TipoGarantia)
                .Include(g => g.PrestamoGarantias)
                .AsQueryable();

            if (personaId.HasValue)
            {
                query = query.Where(g => g.PersonaId == personaId.Value);
            }

            var garantias = await query.ToListAsync();

            // Calcular el valor disponible de cada garantía
            var garantiasDisponibles = garantias.Select(g =>
            {
                // Sumar todos los montos comprometidos en préstamos activos
                var montoComprometido = g.PrestamoGarantias
                    .Where(pg => pg.PrestamoGarantiaEstaActiva)
                    .Sum(pg => pg.PrestamoGarantiaMontoComprometido);

                var valorRealizable = g.GarantiaValorRealizable ?? 0;
                var valorDisponible = valorRealizable - montoComprometido;

                return new GarantiaDisponibleDto
                {
                    GarantiaId = g.GarantiaId,
                    PersonaId = g.PersonaId,
                    PersonaNombreCompleto = g.Persona?.PersonaNombreCompleto ?? "",
                    PersonaFiadorId = g.PersonaFiadorId,
                    PersonaFiadorNombreCompleto = g.PersonaFiador?.PersonaNombreCompleto,
                    TipoGarantiaId = g.TipoGarantiaId,
                    TipoGarantiaNombre = g.TipoGarantia?.TipoGarantiaNombre ?? "",
                    GarantiaDescripcion = g.GarantiaDescripcion,
                    GarantiaValorComercial = g.GarantiaValorComercial,
                    GarantiaValorRealizable = g.GarantiaValorRealizable,
                    MontoComprometido = montoComprometido,
                    ValorDisponible = valorDisponible,
                    GarantiaObservaciones = g.GarantiaObservaciones
                };
            })
            .Where(g => g.ValorDisponible > 0 || g.TipoGarantiaNombre.ToUpper().Contains("FIDUCIARIA")) // Mostrar solo garantías con valor disponible, o fiduciarias (que no tienen límite)
            .OrderByDescending(g => g.ValorDisponible)
            .ToList();

            var totalRecords = garantiasDisponibles.Count;
            var paginatedGarantias = garantiasDisponibles
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            Response.Headers.Add("X-Page-Number", pageNumber.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(paginatedGarantias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las garantías disponibles");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    private async Task<bool> GarantiaExists(long id)
    {
        return await _context.Garantias.AnyAsync(e => e.GarantiaId == id);
    }
}
