using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace AlphaCredit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FondosController : ControllerBase
{
    private readonly AlphaCreditDbContext _context;
    private readonly ILogger<FondosController> _logger;

    public FondosController(AlphaCreditDbContext context, ILogger<FondosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/fondos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Fondo>>> GetFondos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] long? tipoFondoId = null,
        [FromQuery] long? monedaId = null,
        [FromQuery] bool? soloActivos = null)
    {
        try
        {
            var query = _context.Fondos
                .Include(f => f.TipoFondo)
                .Include(f => f.Moneda)
                .AsQueryable();

            // Filtros opcionales
            if (tipoFondoId.HasValue)
            {
                query = query.Where(f => f.TipoFondoId == tipoFondoId.Value);
            }

            if (monedaId.HasValue)
            {
                query = query.Where(f => f.MonedaId == monedaId.Value);
            }

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(f => f.FondoEstaActivo);
            }

            var totalRecords = await query.CountAsync();

            var fondos = await query
                .OrderBy(f => f.FondoNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            Response.Headers.Add("X-Page-Number", pageNumber.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(fondos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los fondos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/fondos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Fondo>> GetFondo(long id)
    {
        try
        {
            var fondo = await _context.Fondos
                .Include(f => f.TipoFondo)
                .Include(f => f.Moneda)
                .Include(f => f.FondoMovimientos)
                .FirstOrDefaultAsync(f => f.FondoId == id);

            if (fondo == null)
            {
                return NotFound(new { message = $"Fondo con ID {id} no encontrado" });
            }

            return Ok(fondo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el fondo {FondoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/fondos
    [HttpPost]
    public async Task<ActionResult<Fondo>> CreateFondo([FromBody] FondoCreateRequest request)
    {
        try
        {
            // Validar que no exista un fondo con el mismo nombre
            var existingFondo = await _context.Fondos
                .FirstOrDefaultAsync(f => f.FondoNombre == request.FondoNombre);

            if (existingFondo != null)
            {
                return BadRequest(new { message = $"Ya existe un fondo con el nombre '{request.FondoNombre}'" });
            }

            // Validar que el tipo de fondo existe
            var tipoFondo = await _context.TiposFondo.FindAsync(request.TipoFondoId);
            if (tipoFondo == null)
            {
                return BadRequest(new { message = "El tipo de fondo especificado no existe" });
            }

            // Validar que la moneda existe
            var moneda = await _context.Monedas.FindAsync(request.MonedaId);
            if (moneda == null)
            {
                return BadRequest(new { message = "La moneda especificada no existe" });
            }

            // Crear el fondo
            var fondo = new Fondo
            {
                FondoNombre = request.FondoNombre,
                FondoDescripcion = request.FondoDescripcion,
                TipoFondoId = request.TipoFondoId,
                MonedaId = request.MonedaId,
                FondoSaldoInicial = request.FondoSaldoInicial,
                FondoSaldoActual = request.FondoSaldoInicial,
                FondoEstaActivo = true,
                FondoUserCrea = request.FondoUserCrea ?? "system",
                FondoFechaCreacion = DateTime.UtcNow
            };

            _context.Fondos.Add(fondo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFondo), new { id = fondo.FondoId }, fondo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el fondo");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    // PUT: api/fondos/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFondo(long id, Fondo fondo)
    {
        if (id != fondo.FondoId)
        {
            return BadRequest(new { message = "El ID del fondo no coincide" });
        }

        try
        {
            var existingFondo = await _context.Fondos.FindAsync(id);
            if (existingFondo == null)
            {
                return NotFound(new { message = $"Fondo con ID {id} no encontrado" });
            }

            // Validar nombre único (excepto el fondo actual)
            var duplicateName = await _context.Fondos
                .AnyAsync(f => f.FondoNombre == fondo.FondoNombre && f.FondoId != id);

            if (duplicateName)
            {
                return BadRequest(new { message = $"Ya existe otro fondo con el nombre '{fondo.FondoNombre}'" });
            }

            // Actualizar campos
            existingFondo.FondoNombre = fondo.FondoNombre;
            existingFondo.FondoDescripcion = fondo.FondoDescripcion;
            existingFondo.TipoFondoId = fondo.TipoFondoId;
            existingFondo.MonedaId = fondo.MonedaId;
            existingFondo.FondoEstaActivo = fondo.FondoEstaActivo;
            existingFondo.FondoFechaModifica = DateTime.UtcNow;
            existingFondo.FondoUserModifica = fondo.FondoUserModifica;

            // Nota: No se permite modificar los saldos directamente aquí
            // Los saldos se actualizan a través de movimientos o préstamos

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await FondoExists(id))
            {
                return NotFound();
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el fondo {FondoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // DELETE: api/fondos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFondo(long id)
    {
        try
        {
            var fondo = await _context.Fondos
                .Include(f => f.FondoMovimientos)
                .FirstOrDefaultAsync(f => f.FondoId == id);

            if (fondo == null)
            {
                return NotFound(new { message = $"Fondo con ID {id} no encontrado" });
            }

            // Nota: Los fondos no tienen relación directa con préstamos en el modelo actual
            // Si se desactiva, se hace por completo

            // Verificar si tiene movimientos
            if (fondo.FondoMovimientos.Any())
            {
                // Soft delete - desactivar en lugar de eliminar
                fondo.FondoEstaActivo = false;
                fondo.FondoFechaModifica = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "El fondo ha sido desactivado porque tiene movimientos registrados" });
            }

            _context.Fondos.Remove(fondo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el fondo {FondoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/fondos/activos
    [HttpGet("activos")]
    public async Task<ActionResult<IEnumerable<Fondo>>> GetFondosActivos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Fondos
                .Where(f => f.FondoEstaActivo)
                .Include(f => f.TipoFondo)
                .Include(f => f.Moneda)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var fondos = await query
                .OrderBy(f => f.FondoNombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            Response.Headers.Add("X-Page-Number", pageNumber.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(fondos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los fondos activos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/fondos/disponibles
    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<Fondo>>> GetFondosDisponibles(
        [FromQuery] decimal? montoMinimo = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Fondos
                .Where(f => f.FondoEstaActivo && f.FondoSaldoActual > 0)
                .Include(f => f.TipoFondo)
                .Include(f => f.Moneda)
                .AsQueryable();

            if (montoMinimo.HasValue)
            {
                query = query.Where(f => f.FondoSaldoActual >= montoMinimo.Value);
            }

            var totalRecords = await query.CountAsync();

            var fondos = await query
                .OrderByDescending(f => f.FondoSaldoActual)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalRecords.ToString());
            Response.Headers.Add("X-Page-Number", pageNumber.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(fondos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los fondos disponibles");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/fondos/5/saldo
    [HttpGet("{id}/saldo")]
    public async Task<ActionResult<object>> GetSaldoFondo(long id)
    {
        try
        {
            var fondo = await _context.Fondos.FindAsync(id);
            if (fondo == null)
            {
                return NotFound(new { message = $"Fondo con ID {id} no encontrado" });
            }

            // Calcular movimientos del fondo
            var totalIngresos = await _context.FondosMovimientos
                .Where(m => m.FondoId == id && m.FondoMovimientoTipo == "INGRESO")
                .SumAsync(m => (decimal?)m.FondoMovimientoMonto) ?? 0;

            var totalEgresos = await _context.FondosMovimientos
                .Where(m => m.FondoId == id && m.FondoMovimientoTipo == "EGRESO")
                .SumAsync(m => (decimal?)m.FondoMovimientoMonto) ?? 0;

            return Ok(new
            {
                fondoId = fondo.FondoId,
                fondoNombre = fondo.FondoNombre,
                saldoInicial = fondo.FondoSaldoInicial,
                saldoActual = fondo.FondoSaldoActual,
                totalIngresos,
                totalEgresos,
                saldoCalculado = fondo.FondoSaldoInicial + totalIngresos - totalEgresos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el saldo del fondo {FondoId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/fondos/movimientos
    [HttpPost("movimientos")]
    public async Task<ActionResult<FondoMovimiento>> CreateMovimientoFondo([FromBody] FondoMovimientoRequest request)
    {
        try
        {
            // Validar que el fondo existe
            var fondo = await _context.Fondos.FindAsync(request.FondoId);
            if (fondo == null)
            {
                return BadRequest(new { message = "El fondo especificado no existe" });
            }

            if (!fondo.FondoEstaActivo)
            {
                return BadRequest(new { message = "El fondo no está activo" });
            }

            // Validar tipo de movimiento
            if (request.FondoMovimientoTipo != "INGRESO" && request.FondoMovimientoTipo != "EGRESO")
            {
                return BadRequest(new { message = "El tipo de movimiento debe ser INGRESO o EGRESO" });
            }

            // Validar monto
            if (request.FondoMovimientoMonto <= 0)
            {
                return BadRequest(new { message = "El monto debe ser mayor a cero" });
            }

            // Validar que no quede en negativo si es egreso
            if (request.FondoMovimientoTipo == "EGRESO")
            {
                if (fondo.FondoSaldoActual < request.FondoMovimientoMonto)
                {
                    return BadRequest(new
                    {
                        message = $"Saldo insuficiente. Saldo actual: {fondo.FondoSaldoActual:C}, Monto solicitado: {request.FondoMovimientoMonto:C}",
                        saldoActual = fondo.FondoSaldoActual,
                        montoSolicitado = request.FondoMovimientoMonto
                    });
                }
            }

            // Crear el movimiento
            var movimiento = new FondoMovimiento
            {
                FondoId = request.FondoId,
                FondoMovimientoTipo = request.FondoMovimientoTipo,
                FondoMovimientoMonto = request.FondoMovimientoMonto,
                FondoMovimientoConcepto = request.FondoMovimientoConcepto,
                FondoMovimientoFecha = request.FondoMovimientoFecha ?? DateTime.UtcNow,
                FondoMovimientoUserCrea = request.FondoMovimientoUserCrea ?? "system",
                FondoMovimientoFechaCreacion = DateTime.UtcNow
            };

            _context.FondosMovimientos.Add(movimiento);

            // Actualizar saldo del fondo
            if (request.FondoMovimientoTipo == "INGRESO")
            {
                fondo.FondoSaldoActual += request.FondoMovimientoMonto;
            }
            else // EGRESO
            {
                fondo.FondoSaldoActual -= request.FondoMovimientoMonto;
            }

            fondo.FondoFechaModifica = DateTime.UtcNow;
            fondo.FondoUserModifica = request.FondoMovimientoUserCrea ?? "system";

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFondo), new { id = fondo.FondoId }, movimiento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el movimiento de fondo");
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    private async Task<bool> FondoExists(long id)
    {
        return await _context.Fondos.AnyAsync(e => e.FondoId == id);
    }
}

// DTO para crear fondos
public class FondoCreateRequest
{
    [Required(ErrorMessage = "El nombre del fondo es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string FondoNombre { get; set; } = string.Empty;

    [MaxLength(300, ErrorMessage = "La descripción no puede exceder 300 caracteres")]
    public string? FondoDescripcion { get; set; }

    [Required(ErrorMessage = "El tipo de fondo es requerido")]
    [Range(1, long.MaxValue, ErrorMessage = "Debe seleccionar un tipo de fondo válido")]
    public long TipoFondoId { get; set; }

    [Required(ErrorMessage = "La moneda es requerida")]
    [Range(1, long.MaxValue, ErrorMessage = "Debe seleccionar una moneda válida")]
    public long MonedaId { get; set; }

    [Required(ErrorMessage = "El saldo inicial es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El saldo inicial debe ser mayor o igual a cero")]
    public decimal FondoSaldoInicial { get; set; }

    public bool FondoEstaActivo { get; set; } = true;

    [MaxLength(40, ErrorMessage = "El usuario creador no puede exceder 40 caracteres")]
    public string? FondoUserCrea { get; set; }
}

// DTO para crear movimientos de fondo
public class FondoMovimientoRequest
{
    public long FondoId { get; set; }
    public string FondoMovimientoTipo { get; set; } = string.Empty;
    public decimal FondoMovimientoMonto { get; set; }
    public string? FondoMovimientoConcepto { get; set; }
    public DateTime? FondoMovimientoFecha { get; set; }
    public string? FondoMovimientoUserCrea { get; set; }
}
