using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;
using AlphaCredit.Api.Services;

namespace AlphaCredit.Api.Controllers;

[ApiController]
[Route("api/personas/{personaId}/documentos")]
public class PersonaDocumentosController : ControllerBase
{
    private readonly AlphaCreditDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PersonaDocumentosController> _logger;

    public PersonaDocumentosController(
        AlphaCreditDbContext context,
        IFileStorageService fileStorageService,
        ILogger<PersonaDocumentosController> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    // GET: api/personas/5/documentos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonaDocumento>>> GetDocumentos(long personaId)
    {
        try
        {
            var persona = await _context.Personas.FindAsync(personaId);
            if (persona == null)
            {
                return NotFound(new { message = "Persona no encontrada" });
            }

            var documentos = await _context.PersonaDocumentos
                .Where(d => d.PersonaId == personaId)
                .OrderByDescending(d => d.PersonaDocumentoFechaCreacion)
                .ToListAsync();

            return Ok(documentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documentos de la persona {PersonaId}", personaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // POST: api/personas/5/documentos/upload
    [HttpPost("upload")]
    public async Task<ActionResult<PersonaDocumento>> UploadDocumento(
        long personaId,
        [FromForm] IFormFile file,
        [FromForm] string tipoDocumento = "DNI")
    {
        try
        {
            // Validar que la persona existe
            var persona = await _context.Personas.FindAsync(personaId);
            if (persona == null)
            {
                return NotFound(new { message = "Persona no encontrada" });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No se ha proporcionado ningún archivo" });
            }

            // Guardar el archivo
            var filePath = await _fileStorageService.SaveDniFileAsync(file, personaId, tipoDocumento);

            // Crear registro en la base de datos
            var documento = new PersonaDocumento
            {
                PersonaId = personaId,
                PersonaDocumentoPath = filePath,
                PersonaDocumentoTipo = tipoDocumento,
                PersonaDocumentoFechaCreacion = DateTime.UtcNow
            };

            _context.PersonaDocumentos.Add(documento);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Documento subido para persona {PersonaId}: {Path}", personaId, filePath);

            return CreatedAtAction(nameof(GetDocumentos), new { personaId }, documento);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al subir documento");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir documento para persona {PersonaId}", personaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // POST: api/personas/5/documentos/upload-multiple
    [HttpPost("upload-multiple")]
    public async Task<ActionResult<IEnumerable<PersonaDocumento>>> UploadMultipleDocumentos(
        long personaId,
        [FromForm] List<IFormFile> files,
        [FromForm] string tipoDocumento = "DNI")
    {
        try
        {
            // Validar que la persona existe
            var persona = await _context.Personas.FindAsync(personaId);
            if (persona == null)
            {
                return NotFound(new { message = "Persona no encontrada" });
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "No se han proporcionado archivos" });
            }

            var documentos = new List<PersonaDocumento>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    try
                    {
                        // Guardar el archivo
                        var filePath = await _fileStorageService.SaveDniFileAsync(file, personaId, tipoDocumento);

                        // Crear registro en la base de datos
                        var documento = new PersonaDocumento
                        {
                            PersonaId = personaId,
                            PersonaDocumentoPath = filePath,
                            PersonaDocumentoTipo = tipoDocumento,
                            PersonaDocumentoFechaCreacion = DateTime.UtcNow
                        };

                        _context.PersonaDocumentos.Add(documento);
                        documentos.Add(documento);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error al subir archivo {FileName}", file.FileName);
                        // Continuar con el siguiente archivo
                    }
                }
            }

            if (documentos.Count == 0)
            {
                return BadRequest(new { message = "No se pudo subir ningún archivo" });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Se subieron {Count} documentos para persona {PersonaId}",
                documentos.Count,
                personaId
            );

            return Ok(new {
                message = $"Se subieron {documentos.Count} de {files.Count} archivos",
                documentos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir múltiples documentos para persona {PersonaId}", personaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // GET: api/personas/5/documentos/1/download
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocumento(long personaId, long id)
    {
        try
        {
            var documento = await _context.PersonaDocumentos
                .FirstOrDefaultAsync(d => d.PersonaDocumentoId == id && d.PersonaId == personaId);

            if (documento == null)
            {
                return NotFound(new { message = "Documento no encontrado" });
            }

            var fullPath = _fileStorageService.GetFullPath(documento.PersonaDocumentoPath);

            if (!_fileStorageService.FileExists(documento.PersonaDocumentoPath))
            {
                _logger.LogWarning("Archivo físico no encontrado: {Path}", fullPath);
                return NotFound(new { message = "Archivo no encontrado en el servidor" });
            }

            var extension = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            var fileName = Path.GetFileName(fullPath);

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar documento {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // DELETE: api/personas/5/documentos/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocumento(long personaId, long id)
    {
        try
        {
            var documento = await _context.PersonaDocumentos
                .FirstOrDefaultAsync(d => d.PersonaDocumentoId == id && d.PersonaId == personaId);

            if (documento == null)
            {
                return NotFound(new { message = "Documento no encontrado" });
            }

            // Eliminar archivo físico
            await _fileStorageService.DeleteFileAsync(documento.PersonaDocumentoPath);

            // Eliminar registro de la base de datos
            _context.PersonaDocumentos.Remove(documento);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Documento {Id} eliminado para persona {PersonaId}", id, personaId);

            return Ok(new { message = "Documento eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar documento {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
