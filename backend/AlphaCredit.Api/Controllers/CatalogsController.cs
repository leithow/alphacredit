using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;

namespace AlphaCredit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogsController : ControllerBase
{
    private readonly AlphaCreditDbContext _context;
    private readonly ILogger<CatalogsController> _logger;

    public CatalogsController(AlphaCreditDbContext context, ILogger<CatalogsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Sexo

    // GET: api/catalogs/sexo
    [HttpGet("sexo")]
    public async Task<ActionResult<IEnumerable<Sexo>>> GetSexos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.Sexos.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(s => s.SexoEstaActivo);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.SexoNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de Sexo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/sexo/5
    [HttpGet("sexo/{id}")]
    public async Task<ActionResult<Sexo>> GetSexo(long id)
    {
        try
        {
            var item = await _context.Sexos.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"Sexo con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Sexo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/sexo
    [HttpPost("sexo")]
    public async Task<ActionResult<Sexo>> CreateSexo(Sexo sexo)
    {
        try
        {
            var exists = await _context.Sexos.AnyAsync(s => s.SexoNombre == sexo.SexoNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{sexo.SexoNombre}'" });
            }

            sexo.SexoEstaActivo = true;
            _context.Sexos.Add(sexo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSexo), new { id = sexo.SexoId }, sexo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear Sexo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/sexo/5
    [HttpPut("sexo/{id}")]
    public async Task<IActionResult> UpdateSexo(long id, Sexo sexo)
    {
        if (id != sexo.SexoId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.Sexos.AnyAsync(s => s.SexoNombre == sexo.SexoNombre && s.SexoId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{sexo.SexoNombre}'" });
            }

            _context.Entry(sexo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Sexos.AnyAsync(e => e.SexoId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar Sexo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/sexo/5
    [HttpDelete("sexo/{id}")]
    public async Task<IActionResult> DeleteSexo(long id)
    {
        try
        {
            var item = await _context.Sexos.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"Sexo con ID {id} no encontrado" });
            }

            var enUso = await _context.Personas.AnyAsync(p => p.SexoId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.Sexos.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar Sexo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region EstadoCivil

    // GET: api/catalogs/estadocivil
    [HttpGet("estadocivil")]
    public async Task<ActionResult<IEnumerable<EstadoCivil>>> GetEstadosCiviles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.EstadosCiviles.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(e => e.EstadoCivilEstaActivo);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(e => e.EstadoCivilNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de EstadoCivil");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/estadocivil/5
    [HttpGet("estadocivil/{id}")]
    public async Task<ActionResult<EstadoCivil>> GetEstadoCivil(long id)
    {
        try
        {
            var item = await _context.EstadosCiviles.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"EstadoCivil con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener EstadoCivil {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/estadocivil
    [HttpPost("estadocivil")]
    public async Task<ActionResult<EstadoCivil>> CreateEstadoCivil(EstadoCivil estadoCivil)
    {
        try
        {
            var exists = await _context.EstadosCiviles.AnyAsync(e => e.EstadoCivilNombre == estadoCivil.EstadoCivilNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{estadoCivil.EstadoCivilNombre}'" });
            }

            estadoCivil.EstadoCivilEstaActivo = true;
            _context.EstadosCiviles.Add(estadoCivil);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEstadoCivil), new { id = estadoCivil.EstadoCivilId }, estadoCivil);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear EstadoCivil");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/estadocivil/5
    [HttpPut("estadocivil/{id}")]
    public async Task<IActionResult> UpdateEstadoCivil(long id, EstadoCivil estadoCivil)
    {
        if (id != estadoCivil.EstadoCivilId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.EstadosCiviles.AnyAsync(e => e.EstadoCivilNombre == estadoCivil.EstadoCivilNombre && e.EstadoCivilId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{estadoCivil.EstadoCivilNombre}'" });
            }

            _context.Entry(estadoCivil).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.EstadosCiviles.AnyAsync(e => e.EstadoCivilId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar EstadoCivil {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/estadocivil/5
    [HttpDelete("estadocivil/{id}")]
    public async Task<IActionResult> DeleteEstadoCivil(long id)
    {
        try
        {
            var item = await _context.EstadosCiviles.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"EstadoCivil con ID {id} no encontrado" });
            }

            var enUso = await _context.Personas.AnyAsync(p => p.EstadoCivilId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.EstadosCiviles.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar EstadoCivil {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region TipoIdentificacion

    // GET: api/catalogs/tipoidentificacion
    [HttpGet("tipoidentificacion")]
    public async Task<ActionResult<IEnumerable<TipoIdentificacion>>> GetTiposIdentificacion(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.TiposIdentificacion.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(t => t.TipoIdentificacionEstaActivo);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.TipoIdentificacionNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de TipoIdentificacion");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/tipoidentificacion/5
    [HttpGet("tipoidentificacion/{id}")]
    public async Task<ActionResult<TipoIdentificacion>> GetTipoIdentificacion(long id)
    {
        try
        {
            var item = await _context.TiposIdentificacion.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"TipoIdentificacion con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener TipoIdentificacion {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/tipoidentificacion
    [HttpPost("tipoidentificacion")]
    public async Task<ActionResult<TipoIdentificacion>> CreateTipoIdentificacion(TipoIdentificacion tipoIdentificacion)
    {
        try
        {
            var exists = await _context.TiposIdentificacion.AnyAsync(t => t.TipoIdentificacionNombre == tipoIdentificacion.TipoIdentificacionNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{tipoIdentificacion.TipoIdentificacionNombre}'" });
            }

            tipoIdentificacion.TipoIdentificacionEstaActivo = true;
            _context.TiposIdentificacion.Add(tipoIdentificacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTipoIdentificacion), new { id = tipoIdentificacion.TipoIdentificacionId }, tipoIdentificacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear TipoIdentificacion");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/tipoidentificacion/5
    [HttpPut("tipoidentificacion/{id}")]
    public async Task<IActionResult> UpdateTipoIdentificacion(long id, TipoIdentificacion tipoIdentificacion)
    {
        if (id != tipoIdentificacion.TipoIdentificacionId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.TiposIdentificacion.AnyAsync(t => t.TipoIdentificacionNombre == tipoIdentificacion.TipoIdentificacionNombre && t.TipoIdentificacionId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{tipoIdentificacion.TipoIdentificacionNombre}'" });
            }

            _context.Entry(tipoIdentificacion).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.TiposIdentificacion.AnyAsync(e => e.TipoIdentificacionId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar TipoIdentificacion {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/tipoidentificacion/5
    [HttpDelete("tipoidentificacion/{id}")]
    public async Task<IActionResult> DeleteTipoIdentificacion(long id)
    {
        try
        {
            var item = await _context.TiposIdentificacion.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"TipoIdentificacion con ID {id} no encontrado" });
            }

            var enUso = await _context.Personas.AnyAsync(p => p.TipoIdentificacionId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.TiposIdentificacion.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar TipoIdentificacion {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region OperadorTelefono

    // GET: api/catalogs/operadortelefono
    [HttpGet("operadortelefono")]
    public async Task<ActionResult<IEnumerable<OperadorTelefono>>> GetOperadoresTelefono(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.OperadoresTelefono.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(o => o.OperadorTelefonoEstaActivo);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(o => o.OperadorTelefonoNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de OperadorTelefono");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/operadortelefono/5
    [HttpGet("operadortelefono/{id}")]
    public async Task<ActionResult<OperadorTelefono>> GetOperadorTelefono(long id)
    {
        try
        {
            var item = await _context.OperadoresTelefono.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"OperadorTelefono con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener OperadorTelefono {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/operadortelefono
    [HttpPost("operadortelefono")]
    public async Task<ActionResult<OperadorTelefono>> CreateOperadorTelefono(OperadorTelefono operadorTelefono)
    {
        try
        {
            var exists = await _context.OperadoresTelefono.AnyAsync(o => o.OperadorTelefonoNombre == operadorTelefono.OperadorTelefonoNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{operadorTelefono.OperadorTelefonoNombre}'" });
            }

            operadorTelefono.OperadorTelefonoEstaActivo = true;
            _context.OperadoresTelefono.Add(operadorTelefono);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOperadorTelefono), new { id = operadorTelefono.OperadorTelefonoId }, operadorTelefono);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear OperadorTelefono");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/operadortelefono/5
    [HttpPut("operadortelefono/{id}")]
    public async Task<IActionResult> UpdateOperadorTelefono(long id, OperadorTelefono operadorTelefono)
    {
        if (id != operadorTelefono.OperadorTelefonoId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.OperadoresTelefono.AnyAsync(o => o.OperadorTelefonoNombre == operadorTelefono.OperadorTelefonoNombre && o.OperadorTelefonoId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{operadorTelefono.OperadorTelefonoNombre}'" });
            }

            _context.Entry(operadorTelefono).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.OperadoresTelefono.AnyAsync(e => e.OperadorTelefonoId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar OperadorTelefono {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/operadortelefono/5
    [HttpDelete("operadortelefono/{id}")]
    public async Task<IActionResult> DeleteOperadorTelefono(long id)
    {
        try
        {
            var item = await _context.OperadoresTelefono.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"OperadorTelefono con ID {id} no encontrado" });
            }

            var enUso = await _context.PersonaTelefonos.AnyAsync(pt => pt.OperadorTelefonoId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.OperadoresTelefono.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar OperadorTelefono {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region Moneda

    // GET: api/catalogs/moneda
    [HttpGet("moneda")]
    public async Task<ActionResult<IEnumerable<Moneda>>> GetMonedas(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.Monedas.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(m => m.MonedaEstaActiva);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(m => m.MonedaNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de Moneda");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/moneda/5
    [HttpGet("moneda/{id}")]
    public async Task<ActionResult<Moneda>> GetMoneda(long id)
    {
        try
        {
            var item = await _context.Monedas.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"Moneda con ID {id} no encontrada" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Moneda {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/moneda
    [HttpPost("moneda")]
    public async Task<ActionResult<Moneda>> CreateMoneda(Moneda moneda)
    {
        try
        {
            var exists = await _context.Monedas.AnyAsync(m => m.MonedaCodigo == moneda.MonedaCodigo);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe una moneda con el código '{moneda.MonedaCodigo}'" });
            }

            moneda.MonedaEstaActiva = true;
            _context.Monedas.Add(moneda);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMoneda), new { id = moneda.MonedaId }, moneda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear Moneda");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/moneda/5
    [HttpPut("moneda/{id}")]
    public async Task<IActionResult> UpdateMoneda(long id, Moneda moneda)
    {
        if (id != moneda.MonedaId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.Monedas.AnyAsync(m => m.MonedaCodigo == moneda.MonedaCodigo && m.MonedaId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otra moneda con el código '{moneda.MonedaCodigo}'" });
            }

            _context.Entry(moneda).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Monedas.AnyAsync(e => e.MonedaId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar Moneda {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/moneda/5
    [HttpDelete("moneda/{id}")]
    public async Task<IActionResult> DeleteMoneda(long id)
    {
        try
        {
            var item = await _context.Monedas.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"Moneda con ID {id} no encontrada" });
            }

            var enUsoPrestamos = await _context.Prestamos.AnyAsync(p => p.MonedaId == id);
            var enUsoFondos = await _context.Fondos.AnyAsync(f => f.MonedaId == id);

            if (enUsoPrestamos || enUsoFondos)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.Monedas.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar Moneda {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region FrecuenciaPago

    // GET: api/catalogs/frecuenciapago
    [HttpGet("frecuenciapago")]
    public async Task<ActionResult<IEnumerable<FrecuenciaPago>>> GetFrecuenciasPago(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.FrecuenciasPago.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(f => f.FrecuenciaPagoEstaActiva);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(f => f.FrecuenciaPagoDias)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de FrecuenciaPago");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/frecuenciapago/5
    [HttpGet("frecuenciapago/{id}")]
    public async Task<ActionResult<FrecuenciaPago>> GetFrecuenciaPago(long id)
    {
        try
        {
            var item = await _context.FrecuenciasPago.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"FrecuenciaPago con ID {id} no encontrada" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener FrecuenciaPago {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/frecuenciapago
    [HttpPost("frecuenciapago")]
    public async Task<ActionResult<FrecuenciaPago>> CreateFrecuenciaPago(FrecuenciaPago frecuenciaPago)
    {
        try
        {
            var exists = await _context.FrecuenciasPago.AnyAsync(f => f.FrecuenciaPagoNombre == frecuenciaPago.FrecuenciaPagoNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{frecuenciaPago.FrecuenciaPagoNombre}'" });
            }

            frecuenciaPago.FrecuenciaPagoEstaActiva = true;
            _context.FrecuenciasPago.Add(frecuenciaPago);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFrecuenciaPago), new { id = frecuenciaPago.FrecuenciaPagoId }, frecuenciaPago);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear FrecuenciaPago");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/frecuenciapago/5
    [HttpPut("frecuenciapago/{id}")]
    public async Task<IActionResult> UpdateFrecuenciaPago(long id, FrecuenciaPago frecuenciaPago)
    {
        if (id != frecuenciaPago.FrecuenciaPagoId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.FrecuenciasPago.AnyAsync(f => f.FrecuenciaPagoNombre == frecuenciaPago.FrecuenciaPagoNombre && f.FrecuenciaPagoId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{frecuenciaPago.FrecuenciaPagoNombre}'" });
            }

            _context.Entry(frecuenciaPago).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.FrecuenciasPago.AnyAsync(e => e.FrecuenciaPagoId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar FrecuenciaPago {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/frecuenciapago/5
    [HttpDelete("frecuenciapago/{id}")]
    public async Task<IActionResult> DeleteFrecuenciaPago(long id)
    {
        try
        {
            var item = await _context.FrecuenciasPago.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"FrecuenciaPago con ID {id} no encontrada" });
            }

            var enUso = await _context.Prestamos.AnyAsync(p => p.FrecuenciaPagoId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.FrecuenciasPago.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar FrecuenciaPago {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region FormaPago

    // GET: api/catalogs/formapago
    [HttpGet("formapago")]
    public async Task<ActionResult<IEnumerable<FormaPago>>> GetFormasPago(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.FormasPago.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(f => f.FormaPagoEstaActiva);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(f => f.FormaPagoNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de FormaPago");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/formapago/5
    [HttpGet("formapago/{id}")]
    public async Task<ActionResult<FormaPago>> GetFormaPago(long id)
    {
        try
        {
            var item = await _context.FormasPago.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"FormaPago con ID {id} no encontrada" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener FormaPago {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/formapago
    [HttpPost("formapago")]
    public async Task<ActionResult<FormaPago>> CreateFormaPago(FormaPago formaPago)
    {
        try
        {
            var exists = await _context.FormasPago.AnyAsync(f => f.FormaPagoNombre == formaPago.FormaPagoNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{formaPago.FormaPagoNombre}'" });
            }

            formaPago.FormaPagoEstaActiva = true;
            _context.FormasPago.Add(formaPago);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFormaPago), new { id = formaPago.FormaPagoId }, formaPago);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear FormaPago");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/formapago/5
    [HttpPut("formapago/{id}")]
    public async Task<IActionResult> UpdateFormaPago(long id, FormaPago formaPago)
    {
        if (id != formaPago.FormaPagoId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.FormasPago.AnyAsync(f => f.FormaPagoNombre == formaPago.FormaPagoNombre && f.FormaPagoId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{formaPago.FormaPagoNombre}'" });
            }

            _context.Entry(formaPago).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.FormasPago.AnyAsync(e => e.FormaPagoId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar FormaPago {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/formapago/5
    [HttpDelete("formapago/{id}")]
    public async Task<IActionResult> DeleteFormaPago(long id)
    {
        try
        {
            var item = await _context.FormasPago.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"FormaPago con ID {id} no encontrada" });
            }

            var enUso = await _context.MovimientosPrestamo.AnyAsync(mp => mp.FormaPagoId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.FormasPago.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar FormaPago {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region DestinoCredito

    // GET: api/catalogs/destinocredito
    [HttpGet("destinocredito")]
    public async Task<ActionResult<IEnumerable<DestinoCredito>>> GetDestinosCredito(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.DestinosCredito.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(d => d.DestinoCreditoEstaActivo);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(d => d.DestinoCreditoNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de DestinoCredito");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/destinocredito/5
    [HttpGet("destinocredito/{id}")]
    public async Task<ActionResult<DestinoCredito>> GetDestinoCredito(long id)
    {
        try
        {
            var item = await _context.DestinosCredito.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"DestinoCredito con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener DestinoCredito {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/destinocredito
    [HttpPost("destinocredito")]
    public async Task<ActionResult<DestinoCredito>> CreateDestinoCredito(DestinoCredito destinoCredito)
    {
        try
        {
            var exists = await _context.DestinosCredito.AnyAsync(d => d.DestinoCreditoNombre == destinoCredito.DestinoCreditoNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{destinoCredito.DestinoCreditoNombre}'" });
            }

            destinoCredito.DestinoCreditoEstaActivo = true;
            _context.DestinosCredito.Add(destinoCredito);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDestinoCredito), new { id = destinoCredito.DestinoCreditoId }, destinoCredito);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear DestinoCredito");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/destinocredito/5
    [HttpPut("destinocredito/{id}")]
    public async Task<IActionResult> UpdateDestinoCredito(long id, DestinoCredito destinoCredito)
    {
        if (id != destinoCredito.DestinoCreditoId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.DestinosCredito.AnyAsync(d => d.DestinoCreditoNombre == destinoCredito.DestinoCreditoNombre && d.DestinoCreditoId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{destinoCredito.DestinoCreditoNombre}'" });
            }

            _context.Entry(destinoCredito).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.DestinosCredito.AnyAsync(e => e.DestinoCreditoId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar DestinoCredito {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/destinocredito/5
    [HttpDelete("destinocredito/{id}")]
    public async Task<IActionResult> DeleteDestinoCredito(long id)
    {
        try
        {
            var item = await _context.DestinosCredito.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"DestinoCredito con ID {id} no encontrado" });
            }

            var enUso = await _context.Prestamos.AnyAsync(p => p.DestinoCreditoId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.DestinosCredito.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar DestinoCredito {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region EstadoPrestamo

    // GET: api/catalogs/estadoprestamo
    [HttpGet("estadoprestamo")]
    public async Task<ActionResult<IEnumerable<EstadoPrestamo>>> GetEstadosPrestamo(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.EstadosPrestamo.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(e => e.EstadoPrestamoEstaActivo);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(e => e.EstadoPrestamoNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de EstadoPrestamo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/estadoprestamo/5
    [HttpGet("estadoprestamo/{id}")]
    public async Task<ActionResult<EstadoPrestamo>> GetEstadoPrestamo(long id)
    {
        try
        {
            var item = await _context.EstadosPrestamo.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"EstadoPrestamo con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener EstadoPrestamo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/estadoprestamo
    [HttpPost("estadoprestamo")]
    public async Task<ActionResult<EstadoPrestamo>> CreateEstadoPrestamo(EstadoPrestamo estadoPrestamo)
    {
        try
        {
            var exists = await _context.EstadosPrestamo.AnyAsync(e => e.EstadoPrestamoNombre == estadoPrestamo.EstadoPrestamoNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{estadoPrestamo.EstadoPrestamoNombre}'" });
            }

            estadoPrestamo.EstadoPrestamoEstaActivo = true;
            _context.EstadosPrestamo.Add(estadoPrestamo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEstadoPrestamo), new { id = estadoPrestamo.EstadoPrestamoId }, estadoPrestamo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear EstadoPrestamo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/estadoprestamo/5
    [HttpPut("estadoprestamo/{id}")]
    public async Task<IActionResult> UpdateEstadoPrestamo(long id, EstadoPrestamo estadoPrestamo)
    {
        if (id != estadoPrestamo.EstadoPrestamoId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.EstadosPrestamo.AnyAsync(e => e.EstadoPrestamoNombre == estadoPrestamo.EstadoPrestamoNombre && e.EstadoPrestamoId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{estadoPrestamo.EstadoPrestamoNombre}'" });
            }

            _context.Entry(estadoPrestamo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.EstadosPrestamo.AnyAsync(e => e.EstadoPrestamoId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar EstadoPrestamo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/estadoprestamo/5
    [HttpDelete("estadoprestamo/{id}")]
    public async Task<IActionResult> DeleteEstadoPrestamo(long id)
    {
        try
        {
            var item = await _context.EstadosPrestamo.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"EstadoPrestamo con ID {id} no encontrado" });
            }

            var enUso = await _context.Prestamos.AnyAsync(p => p.EstadoPrestamoId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.EstadosPrestamo.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar EstadoPrestamo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region TipoGarantia

    // GET: api/catalogs/tipogarantia
    [HttpGet("tipogarantia")]
    public async Task<ActionResult<IEnumerable<TipoGarantia>>> GetTiposGarantia(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.TiposGarantia.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(t => t.TipoGarantiaEstaActiva);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.TipoGarantiaNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de TipoGarantia");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/tipogarantia/5
    [HttpGet("tipogarantia/{id}")]
    public async Task<ActionResult<TipoGarantia>> GetTipoGarantia(long id)
    {
        try
        {
            var item = await _context.TiposGarantia.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"TipoGarantia con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener TipoGarantia {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/tipogarantia
    [HttpPost("tipogarantia")]
    public async Task<ActionResult<TipoGarantia>> CreateTipoGarantia(TipoGarantia tipoGarantia)
    {
        try
        {
            var exists = await _context.TiposGarantia.AnyAsync(t => t.TipoGarantiaNombre == tipoGarantia.TipoGarantiaNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{tipoGarantia.TipoGarantiaNombre}'" });
            }

            tipoGarantia.TipoGarantiaEstaActiva = true;
            _context.TiposGarantia.Add(tipoGarantia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTipoGarantia), new { id = tipoGarantia.TipoGarantiaId }, tipoGarantia);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear TipoGarantia");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/tipogarantia/5
    [HttpPut("tipogarantia/{id}")]
    public async Task<IActionResult> UpdateTipoGarantia(long id, TipoGarantia tipoGarantia)
    {
        if (id != tipoGarantia.TipoGarantiaId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.TiposGarantia.AnyAsync(t => t.TipoGarantiaNombre == tipoGarantia.TipoGarantiaNombre && t.TipoGarantiaId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{tipoGarantia.TipoGarantiaNombre}'" });
            }

            _context.Entry(tipoGarantia).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.TiposGarantia.AnyAsync(e => e.TipoGarantiaId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar TipoGarantia {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/tipogarantia/5
    [HttpDelete("tipogarantia/{id}")]
    public async Task<IActionResult> DeleteTipoGarantia(long id)
    {
        try
        {
            var item = await _context.TiposGarantia.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"TipoGarantia con ID {id} no encontrado" });
            }

            var enUso = await _context.Garantias.AnyAsync(g => g.TipoGarantiaId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.TiposGarantia.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar TipoGarantia {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region EstadoGarantia

    // GET: api/catalogs/estadogarantia
    [HttpGet("estadogarantia")]
    public async Task<ActionResult<IEnumerable<EstadoGarantia>>> GetEstadosGarantia(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.EstadosGarantia.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(e => e.EstadoGarantiaEstaActiva);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(e => e.EstadoGarantiaNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de EstadoGarantia");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/estadogarantia/5
    [HttpGet("estadogarantia/{id}")]
    public async Task<ActionResult<EstadoGarantia>> GetEstadoGarantia(long id)
    {
        try
        {
            var item = await _context.EstadosGarantia.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"EstadoGarantia con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener EstadoGarantia {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/estadogarantia
    [HttpPost("estadogarantia")]
    public async Task<ActionResult<EstadoGarantia>> CreateEstadoGarantia(EstadoGarantia estadoGarantia)
    {
        try
        {
            var exists = await _context.EstadosGarantia.AnyAsync(e => e.EstadoGarantiaNombre == estadoGarantia.EstadoGarantiaNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{estadoGarantia.EstadoGarantiaNombre}'" });
            }

            estadoGarantia.EstadoGarantiaEstaActiva = true;
            _context.EstadosGarantia.Add(estadoGarantia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEstadoGarantia), new { id = estadoGarantia.EstadoGarantiaId }, estadoGarantia);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear EstadoGarantia");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/estadogarantia/5
    [HttpPut("estadogarantia/{id}")]
    public async Task<IActionResult> UpdateEstadoGarantia(long id, EstadoGarantia estadoGarantia)
    {
        if (id != estadoGarantia.EstadoGarantiaId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.EstadosGarantia.AnyAsync(e => e.EstadoGarantiaNombre == estadoGarantia.EstadoGarantiaNombre && e.EstadoGarantiaId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{estadoGarantia.EstadoGarantiaNombre}'" });
            }

            _context.Entry(estadoGarantia).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.EstadosGarantia.AnyAsync(e => e.EstadoGarantiaId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar EstadoGarantia {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/estadogarantia/5
    [HttpDelete("estadogarantia/{id}")]
    public async Task<IActionResult> DeleteEstadoGarantia(long id)
    {
        try
        {
            var item = await _context.EstadosGarantia.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"EstadoGarantia con ID {id} no encontrado" });
            }

            var enUso = await _context.Garantias.AnyAsync(g => g.EstadoGarantiaId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.EstadosGarantia.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar EstadoGarantia {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region ComponentePrestamo

    // GET: api/catalogs/componenteprestamo
    [HttpGet("componenteprestamo")]
    public async Task<ActionResult<IEnumerable<ComponentePrestamo>>> GetComponentesPrestamo(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.ComponentesPrestamo.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(c => c.ComponentePrestamoEstaActivo);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.ComponentePrestamoNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de ComponentePrestamo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/componenteprestamo/5
    [HttpGet("componenteprestamo/{id}")]
    public async Task<ActionResult<ComponentePrestamo>> GetComponentePrestamo(long id)
    {
        try
        {
            var item = await _context.ComponentesPrestamo.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"ComponentePrestamo con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ComponentePrestamo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/componenteprestamo
    [HttpPost("componenteprestamo")]
    public async Task<ActionResult<ComponentePrestamo>> CreateComponentePrestamo(ComponentePrestamo componentePrestamo)
    {
        try
        {
            var exists = await _context.ComponentesPrestamo.AnyAsync(c => c.ComponentePrestamoNombre == componentePrestamo.ComponentePrestamoNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{componentePrestamo.ComponentePrestamoNombre}'" });
            }

            componentePrestamo.ComponentePrestamoEstaActivo = true;
            _context.ComponentesPrestamo.Add(componentePrestamo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComponentePrestamo), new { id = componentePrestamo.ComponentePrestamoId }, componentePrestamo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear ComponentePrestamo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/componenteprestamo/5
    [HttpPut("componenteprestamo/{id}")]
    public async Task<IActionResult> UpdateComponentePrestamo(long id, ComponentePrestamo componentePrestamo)
    {
        if (id != componentePrestamo.ComponentePrestamoId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.ComponentesPrestamo.AnyAsync(c => c.ComponentePrestamoNombre == componentePrestamo.ComponentePrestamoNombre && c.ComponentePrestamoId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{componentePrestamo.ComponentePrestamoNombre}'" });
            }

            _context.Entry(componentePrestamo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.ComponentesPrestamo.AnyAsync(e => e.ComponentePrestamoId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar ComponentePrestamo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/componenteprestamo/5
    [HttpDelete("componenteprestamo/{id}")]
    public async Task<IActionResult> DeleteComponentePrestamo(long id)
    {
        try
        {
            var item = await _context.ComponentesPrestamo.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"ComponentePrestamo con ID {id} no encontrado" });
            }

            var enUso = await _context.PrestamosComponentes.AnyAsync(pc => pc.ComponentePrestamoId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.ComponentesPrestamo.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar ComponentePrestamo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region EstadoComponente

    // GET: api/catalogs/estadocomponente
    [HttpGet("estadocomponente")]
    public async Task<ActionResult<IEnumerable<EstadoComponente>>> GetEstadosComponente(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.EstadosComponente.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(e => e.EstadoComponenteEstaActivo);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(e => e.EstadoComponenteNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de EstadoComponente");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/estadocomponente/5
    [HttpGet("estadocomponente/{id}")]
    public async Task<ActionResult<EstadoComponente>> GetEstadoComponente(long id)
    {
        try
        {
            var item = await _context.EstadosComponente.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"EstadoComponente con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener EstadoComponente {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/estadocomponente
    [HttpPost("estadocomponente")]
    public async Task<ActionResult<EstadoComponente>> CreateEstadoComponente(EstadoComponente estadoComponente)
    {
        try
        {
            var exists = await _context.EstadosComponente.AnyAsync(e => e.EstadoComponenteNombre == estadoComponente.EstadoComponenteNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{estadoComponente.EstadoComponenteNombre}'" });
            }

            estadoComponente.EstadoComponenteEstaActivo = true;
            _context.EstadosComponente.Add(estadoComponente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEstadoComponente), new { id = estadoComponente.EstadoComponenteId }, estadoComponente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear EstadoComponente");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/estadocomponente/5
    [HttpPut("estadocomponente/{id}")]
    public async Task<IActionResult> UpdateEstadoComponente(long id, EstadoComponente estadoComponente)
    {
        if (id != estadoComponente.EstadoComponenteId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.EstadosComponente.AnyAsync(e => e.EstadoComponenteNombre == estadoComponente.EstadoComponenteNombre && e.EstadoComponenteId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{estadoComponente.EstadoComponenteNombre}'" });
            }

            _context.Entry(estadoComponente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.EstadosComponente.AnyAsync(e => e.EstadoComponenteId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar EstadoComponente {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/estadocomponente/5
    [HttpDelete("estadocomponente/{id}")]
    public async Task<IActionResult> DeleteEstadoComponente(long id)
    {
        try
        {
            var item = await _context.EstadosComponente.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"EstadoComponente con ID {id} no encontrado" });
            }

            var enUso = await _context.PrestamosComponentes.AnyAsync(pc => pc.EstadoComponenteId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.EstadosComponente.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar EstadoComponente {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region TipoCuenta

    // GET: api/catalogs/tipocuenta
    [HttpGet("tipocuenta")]
    public async Task<ActionResult<IEnumerable<TipoCuenta>>> GetTiposCuenta(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.TiposCuenta.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(t => t.TipoCuentaEstaActiva);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.TipoCuentaNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de TipoCuenta");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/tipocuenta/5
    [HttpGet("tipocuenta/{id}")]
    public async Task<ActionResult<TipoCuenta>> GetTipoCuenta(long id)
    {
        try
        {
            var item = await _context.TiposCuenta.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"TipoCuenta con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener TipoCuenta {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/tipocuenta
    [HttpPost("tipocuenta")]
    public async Task<ActionResult<TipoCuenta>> CreateTipoCuenta(TipoCuenta tipoCuenta)
    {
        try
        {
            var exists = await _context.TiposCuenta.AnyAsync(t => t.TipoCuentaNombre == tipoCuenta.TipoCuentaNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{tipoCuenta.TipoCuentaNombre}'" });
            }

            tipoCuenta.TipoCuentaEstaActiva = true;
            _context.TiposCuenta.Add(tipoCuenta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTipoCuenta), new { id = tipoCuenta.TipoCuentaId }, tipoCuenta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear TipoCuenta");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/tipocuenta/5
    [HttpPut("tipocuenta/{id}")]
    public async Task<IActionResult> UpdateTipoCuenta(long id, TipoCuenta tipoCuenta)
    {
        if (id != tipoCuenta.TipoCuentaId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.TiposCuenta.AnyAsync(t => t.TipoCuentaNombre == tipoCuenta.TipoCuentaNombre && t.TipoCuentaId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{tipoCuenta.TipoCuentaNombre}'" });
            }

            _context.Entry(tipoCuenta).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.TiposCuenta.AnyAsync(e => e.TipoCuentaId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar TipoCuenta {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/tipocuenta/5
    [HttpDelete("tipocuenta/{id}")]
    public async Task<IActionResult> DeleteTipoCuenta(long id)
    {
        try
        {
            var item = await _context.TiposCuenta.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"TipoCuenta con ID {id} no encontrado" });
            }

            var enUso = await _context.CuentasBancarias.AnyAsync(cb => cb.TipoCuentaId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.TiposCuenta.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar TipoCuenta {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion

    #region TipoFondo

    // GET: api/catalogs/tipofondo
    [HttpGet("tipofondo")]
    public async Task<ActionResult<IEnumerable<TipoFondo>>> GetTiposFondo(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.TiposFondo.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(t => t.TipoFondoEstaActivo);
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.TipoFondoNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el catálogo de TipoFondo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/catalogs/tipofondo/5
    [HttpGet("tipofondo/{id}")]
    public async Task<ActionResult<TipoFondo>> GetTipoFondo(long id)
    {
        try
        {
            var item = await _context.TiposFondo.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"TipoFondo con ID {id} no encontrado" });
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener TipoFondo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/catalogs/tipofondo
    [HttpPost("tipofondo")]
    public async Task<ActionResult<TipoFondo>> CreateTipoFondo(TipoFondo tipoFondo)
    {
        try
        {
            var exists = await _context.TiposFondo.AnyAsync(t => t.TipoFondoNombre == tipoFondo.TipoFondoNombre);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe un registro con el nombre '{tipoFondo.TipoFondoNombre}'" });
            }

            tipoFondo.TipoFondoEstaActivo = true;
            _context.TiposFondo.Add(tipoFondo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTipoFondo), new { id = tipoFondo.TipoFondoId }, tipoFondo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear TipoFondo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // PUT: api/catalogs/tipofondo/5
    [HttpPut("tipofondo/{id}")]
    public async Task<IActionResult> UpdateTipoFondo(long id, TipoFondo tipoFondo)
    {
        if (id != tipoFondo.TipoFondoId)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        try
        {
            var exists = await _context.TiposFondo.AnyAsync(t => t.TipoFondoNombre == tipoFondo.TipoFondoNombre && t.TipoFondoId != id);
            if (exists)
            {
                return BadRequest(new { message = $"Ya existe otro registro con el nombre '{tipoFondo.TipoFondoNombre}'" });
            }

            _context.Entry(tipoFondo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.TiposFondo.AnyAsync(e => e.TipoFondoId == id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar TipoFondo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/catalogs/tipofondo/5
    [HttpDelete("tipofondo/{id}")]
    public async Task<IActionResult> DeleteTipoFondo(long id)
    {
        try
        {
            var item = await _context.TiposFondo.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"TipoFondo con ID {id} no encontrado" });
            }

            var enUso = await _context.Fondos.AnyAsync(f => f.TipoFondoId == id);
            if (enUso)
            {
                return BadRequest(new { message = "No se puede eliminar porque está en uso" });
            }

            _context.TiposFondo.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar TipoFondo {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    #endregion
}
