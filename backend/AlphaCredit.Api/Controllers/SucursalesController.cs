using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;
using AlphaCredit.Api.DTOs;

namespace AlphaCredit.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SucursalesController : ControllerBase
{
    private readonly AlphaCreditDbContext _context;
    private readonly ILogger<SucursalesController> _logger;

    public SucursalesController(AlphaCreditDbContext context, ILogger<SucursalesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/sucursales
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sucursal>>> GetSucursales()
    {
        try
        {
            var sucursales = await _context.Sucursales
                .Include(s => s.Empresa)
                .Where(s => s.SucursalEstaActiva)
                .ToListAsync();
            return Ok(sucursales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sucursales");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/sucursales/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Sucursal>> GetSucursal(long id)
    {
        try
        {
            var sucursal = await _context.Sucursales
                .Include(s => s.Empresa)
                .FirstOrDefaultAsync(s => s.SucursalId == id);

            if (sucursal == null)
            {
                return NotFound(new { message = $"Sucursal con ID {id} no encontrada" });
            }
            return Ok(sucursal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sucursal {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/sucursales
    [HttpPost]
    public async Task<ActionResult<Sucursal>> CreateSucursal(SucursalCreateRequest request)
    {
        try
        {
            // Verify empresa exists
            var empresaExists = await _context.Empresas.AnyAsync(e => e.EmpresaId == request.EmpresaId);
            if (!empresaExists)
            {
                return BadRequest(new { message = "La empresa especificada no existe" });
            }

            var sucursal = new Sucursal
            {
                EmpresaId = request.EmpresaId,
                SucursalNombre = request.SucursalNombre,
                SucursalDireccion = request.SucursalDireccion,
                SucursalTelefono = request.SucursalTelefono,
                SucursalEstaActiva = true
            };

            _context.Sucursales.Add(sucursal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSucursal), new { id = sucursal.SucursalId }, sucursal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sucursal");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
