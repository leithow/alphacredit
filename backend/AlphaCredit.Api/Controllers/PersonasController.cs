using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;

namespace AlphaCredit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonasController : ControllerBase
{
    private readonly AlphaCreditDbContext _context;
    private readonly ILogger<PersonasController> _logger;

    public PersonasController(AlphaCreditDbContext context, ILogger<PersonasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/personas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Persona>>> GetPersonas(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var query = _context.Personas
                .Include(p => p.TipoIdentificacion)
                .Include(p => p.EstadoCivil)
                .Include(p => p.Sexo)
                .AsQueryable();

            // Búsqueda por nombre o identificación
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.PersonaNombreCompleto.Contains(searchTerm) ||
                    p.PersonaIdentificacion.Contains(searchTerm) ||
                    p.PersonaEmail.Contains(searchTerm));
            }

            var totalRecords = await query.CountAsync();

            var personas = await query
                .OrderBy(p => p.PersonaNombreCompleto)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            Response.Headers.Add("X-Page-Number", pageNumber.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(personas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las personas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/personas/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Persona>> GetPersona(long id)
    {
        try
        {
            var persona = await _context.Personas
                .Include(p => p.TipoIdentificacion)
                .Include(p => p.EstadoCivil)
                .Include(p => p.Sexo)
                .Include(p => p.PersonaTelefonos)
                .Include(p => p.PersonaActividades)
                .FirstOrDefaultAsync(p => p.PersonaId == id);

            if (persona == null)
            {
                return NotFound(new { message = $"Persona con ID {id} no encontrada" });
            }

            return Ok(persona);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la persona {PersonaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/personas
    [HttpPost]
    public async Task<ActionResult<Persona>> CreatePersona(Persona persona)
    {
        try
        {
            // Validar que no exista una persona con la misma identificación
            var existingPersona = await _context.Personas
                .FirstOrDefaultAsync(p => p.PersonaIdentificacion == persona.PersonaIdentificacion);

            if (existingPersona != null)
            {
                return BadRequest(new { message = $"Ya existe una persona con la identificación {persona.PersonaIdentificacion}" });
            }

            // Generar nombre completo automáticamente
            persona.PersonaNombreCompleto = $"{persona.PersonaPrimerNombre} " +
                $"{persona.PersonaSegundoNombre} {persona.PersonaPrimerApellido} {persona.PersonaSegundoApellido}".Trim();

            persona.PersonaFechaCreacion = DateTime.UtcNow;
            persona.PersonaEstaActiva = true;

            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPersona), new { id = persona.PersonaId }, persona);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la persona");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/personas/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePersona(long id, Persona persona)
    {
        if (id != persona.PersonaId)
        {
            return BadRequest(new { message = "El ID de la persona no coincide" });
        }

        try
        {
            var existingPersona = await _context.Personas.FindAsync(id);
            if (existingPersona == null)
            {
                return NotFound(new { message = $"Persona con ID {id} no encontrada" });
            }

            // Validar identificación única (excepto la persona actual)
            var duplicateIdentification = await _context.Personas
                .AnyAsync(p => p.PersonaIdentificacion == persona.PersonaIdentificacion && p.PersonaId != id);

            if (duplicateIdentification)
            {
                return BadRequest(new { message = $"Ya existe otra persona con la identificación {persona.PersonaIdentificacion}" });
            }

            // Actualizar campos
            existingPersona.PersonaPrimerNombre = persona.PersonaPrimerNombre;
            existingPersona.PersonaSegundoNombre = persona.PersonaSegundoNombre;
            existingPersona.PersonaPrimerApellido = persona.PersonaPrimerApellido;
            existingPersona.PersonaSegundoApellido = persona.PersonaSegundoApellido;
            existingPersona.PersonaIdentificacion = persona.PersonaIdentificacion;
            existingPersona.TipoIdentificacionId = persona.TipoIdentificacionId;
            existingPersona.PersonaFechaNacimiento = persona.PersonaFechaNacimiento;
            existingPersona.EstadoCivilId = persona.EstadoCivilId;
            existingPersona.SexoId = persona.SexoId;
            existingPersona.PersonaDireccion = persona.PersonaDireccion;
            existingPersona.PersonaGeolocalizacion = persona.PersonaGeolocalizacion;
            existingPersona.PersonaEmail = persona.PersonaEmail;
            existingPersona.PersonaEsNatural = persona.PersonaEsNatural;
            existingPersona.PersonaEsEmpleado = persona.PersonaEsEmpleado;
            existingPersona.PersonaEsCliente = persona.PersonaEsCliente;
            existingPersona.PersonaEsProveedor = persona.PersonaEsProveedor;
            existingPersona.PersonaEstaActiva = persona.PersonaEstaActiva;

            // Regenerar nombre completo
            existingPersona.PersonaNombreCompleto = $"{existingPersona.PersonaPrimerNombre} " +
                $"{existingPersona.PersonaSegundoNombre} {existingPersona.PersonaPrimerApellido} {existingPersona.PersonaSegundoApellido}".Trim();

            existingPersona.PersonaFechaModifica = DateTime.UtcNow;
            existingPersona.PersonaUserModifica = persona.PersonaUserModifica;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await PersonaExists(id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la persona {PersonaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/personas/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePersona(long id)
    {
        try
        {
            var persona = await _context.Personas.FindAsync(id);
            if (persona == null)
            {
                return NotFound(new { message = $"Persona con ID {id} no encontrada" });
            }

            // Verificar si tiene préstamos asociados
            var tienePrestamos = await _context.Prestamos.AnyAsync(p => p.PersonaId == id);
            if (tienePrestamos)
            {
                // Soft delete - desactivar en lugar de eliminar
                persona.PersonaEstaActiva = false;
                persona.PersonaFechaModifica = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "La persona ha sido desactivada porque tiene préstamos asociados" });
            }

            _context.Personas.Remove(persona);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la persona {PersonaId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/personas/clientes
    [HttpGet("clientes")]
    public async Task<ActionResult<IEnumerable<Persona>>> GetClientes()
    {
        try
        {
            var clientes = await _context.Personas
                .Where(p => p.PersonaEsCliente && p.PersonaEstaActiva)
                .Include(p => p.TipoIdentificacion)
                .OrderBy(p => p.PersonaNombreCompleto)
                .ToListAsync();

            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los clientes");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    private async Task<bool> PersonaExists(long id)
    {
        return await _context.Personas.AnyAsync(e => e.PersonaId == id);
    }
}
