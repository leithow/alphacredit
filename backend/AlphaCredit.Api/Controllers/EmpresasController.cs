using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;

namespace AlphaCredit.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EmpresasController : ControllerBase
{
    private readonly AlphaCreditDbContext _context;
    private readonly ILogger<EmpresasController> _logger;

    public EmpresasController(AlphaCreditDbContext context, ILogger<EmpresasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/empresas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Empresa>>> GetEmpresas()
    {
        try
        {
            var empresas = await _context.Empresas
                .Where(e => e.EmpresaEstaActiva)
                .ToListAsync();
            return Ok(empresas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/empresas/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Empresa>> GetEmpresa(long id)
    {
        try
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
            {
                return NotFound(new { message = $"Empresa con ID {id} no encontrada" });
            }
            return Ok(empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresa {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // POST: api/empresas
    [HttpPost]
    public async Task<ActionResult<Empresa>> CreateEmpresa(Empresa empresa)
    {
        try
        {
            empresa.EmpresaEstaActiva = true;
            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmpresa), new { id = empresa.EmpresaId }, empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear empresa");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
