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
    private readonly PrestamoDocumentoService _documentoService;

    public PrestamosController(AlphaCreditDbContext context, ILogger<PrestamosController> logger)
    {
        _context = context;
        _logger = logger;
        _amortizacionService = new PrestamoAmortizacionService();
        _documentoService = new PrestamoDocumentoService(context);
    }

    // GET: api/prestamos
    public async Task<ActionResult<IEnumerable<Prestamo>>> GetPrestamos(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] long? personaId = null,
    [FromQuery] long? estadoId = null)
{
    try
    {
        _logger.LogInformation("=== INICIO GetPrestamos ===");
        
        // PASO 1: Query básica SIN includes
        var query = _context.Prestamos.AsQueryable();
        
        _logger.LogInformation("Query inicial creada");

        // Filtros opcionales
        if (personaId.HasValue)
        {
            query = query.Where(p => p.PersonaId == personaId.Value);
            _logger.LogInformation($"Filtro PersonaId aplicado: {personaId.Value}");
        }
        
        if (estadoId.HasValue)
        {
            query = query.Where(p => p.EstadoPrestamoId == estadoId.Value);
            _logger.LogInformation($"Filtro EstadoId aplicado: {estadoId.Value}");
        }

        // PASO 2: Contar SIN includes
        var totalRecords = await query.CountAsync();
        _logger.LogInformation($"Total de registros: {totalRecords}");

        if (totalRecords == 0)
        {
            _logger.LogWarning("No se encontraron registros");
            return Ok(new { 
                data = new List<Prestamo>(), 
                total = 0,
                message = "No se encontraron préstamos con los filtros aplicados"
            });
        }

      //obtener datos con includes y paginacion
        var prestamos = await query
        .Include(p => p.Persona)
        .Include(p => p.EstadoPrestamo)
            .OrderByDescending(p => p.PrestamoFechaDesembolso)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation($"Préstamos obtenidos con include: {prestamos.Count}");

        Response.Headers.Add("X-Total-Count", totalRecords.ToString());
        Response.Headers.Add("X-Page-Number", pageNumber.ToString());
        Response.Headers.Add("X-Page-Size", pageSize.ToString());

        return Ok(prestamos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "ERROR en GetPrestamos: {Message}", ex.Message);
        _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
        
        if (ex.InnerException != null)
        {
            _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
        }
        
        return StatusCode(500, new { 
            error = "Error interno del servidor", 
            details = ex.Message,
            innerError = ex.InnerException?.Message
        });
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

            // Procesar garantías si se proporcionaron
            _logger.LogInformation("Procesando garantías. Request.Garantias: {Garantias}", request.Garantias);
            if (request.Garantias != null && request.Garantias.Any())
            {
                _logger.LogInformation("Se encontraron {Count} garantías para procesar", request.Garantias.Count);
                foreach (var garantiaDto in request.Garantias)
                {
                    _logger.LogInformation("Procesando garantía ID: {GarantiaId}, Monto: {Monto}", garantiaDto.GarantiaId, garantiaDto.MontoComprometido);

                    // Validar que la garantía existe
                    var garantia = await _context.Garantias
                        .Include(g => g.TipoGarantia)
                        .Include(g => g.PrestamoGarantias)
                        .FirstOrDefaultAsync(g => g.GarantiaId == garantiaDto.GarantiaId);

                    if (garantia == null)
                    {
                        _logger.LogWarning("Garantía con ID {GarantiaId} no encontrada", garantiaDto.GarantiaId);
                        return BadRequest(new { message = $"La garantía con ID {garantiaDto.GarantiaId} no existe" });
                    }

                    // Calcular monto comprometido actual
                    var montoComprometidoActual = garantia.PrestamoGarantias
                        .Where(pg => pg.PrestamoGarantiaEstaActiva)
                        .Sum(pg => pg.PrestamoGarantiaMontoComprometido);

                    var valorRealizable = garantia.GarantiaValorRealizable ?? 0;
                    var valorDisponible = valorRealizable - montoComprometidoActual;

                    // Validar que hay valor disponible (excepto para fiduciarias)
                    if (!garantia.TipoGarantia.TipoGarantiaNombre.ToUpper().Contains("FIDUCIARIA"))
                    {
                        if (garantiaDto.MontoComprometido > valorDisponible)
                        {
                            return BadRequest(new
                            {
                                message = $"La garantía '{garantia.GarantiaDescripcion}' no tiene valor disponible suficiente. " +
                                         $"Valor disponible: {valorDisponible:C}, Monto solicitado: {garantiaDto.MontoComprometido:C}",
                                garantiaId = garantia.GarantiaId,
                                valorDisponible,
                                montoSolicitado = garantiaDto.MontoComprometido
                            });
                        }
                    }
                    else
                    {
                        // Si es garantía fiduciaria, validar que el fiador no sea el mismo que el propietario del préstamo
                        if (!garantia.PersonaFiadorId.HasValue)
                        {
                            return BadRequest(new { message = $"La garantía fiduciaria '{garantia.GarantiaDescripcion}' debe tener un fiador asignado" });
                        }

                        if (garantia.PersonaFiadorId == prestamo.PersonaId)
                        {
                            return BadRequest(new { message = $"El fiador de la garantía '{garantia.GarantiaDescripcion}' no puede ser la misma persona que el propietario del préstamo" });
                        }
                    }

                    // Crear la relación préstamo-garantía
                    var prestamoGarantia = new PrestamoGarantia
                    {
                        PrestamoId = prestamo.PrestamoId,
                        GarantiaId = garantiaDto.GarantiaId,
                        PrestamoGarantiaMontoComprometido = garantiaDto.MontoComprometido,
                        PrestamoGarantiaFechaAsignacion = DateTime.UtcNow,
                        PrestamoGarantiaEstaActiva = true
                    };

                    _context.PrestamosGarantias.Add(prestamoGarantia);
                    _logger.LogInformation("PrestamoGarantia agregada al contexto. PrestamoId: {PrestamoId}, GarantiaId: {GarantiaId}, Monto: {Monto}",
                        prestamo.PrestamoId, garantiaDto.GarantiaId, garantiaDto.MontoComprometido);
                }
            }
            else
            {
                _logger.LogInformation("No se proporcionaron garantías para este préstamo");
            }

            _logger.LogInformation("Guardando cambios en la base de datos (incluyendo garantías)...");
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cambios guardados exitosamente");

            // Generar automáticamente contrato y pagaré
            try
            {
                _logger.LogInformation("Generando documentos para préstamo {PrestamoId}", prestamo.PrestamoId);

                // Generar contrato
                await _documentoService.GenerarContratoPrestamo(prestamo.PrestamoId);
                await _documentoService.RegistrarImpresion(
                    prestamo.PrestamoId,
                    "CONTRATO",
                    request.PrestamoUserCrea ?? "system",
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                // Generar pagaré
                await _documentoService.GenerarPagare(prestamo.PrestamoId);
                await _documentoService.RegistrarImpresion(
                    prestamo.PrestamoId,
                    "PAGARE",
                    request.PrestamoUserCrea ?? "system",
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                _logger.LogInformation("Documentos generados exitosamente para préstamo {PrestamoId}", prestamo.PrestamoId);
            }
            catch (Exception docEx)
            {
                _logger.LogWarning(docEx, "Error al generar documentos automáticos para préstamo {PrestamoId}. El préstamo se creó correctamente.", prestamo.PrestamoId);
                // No fallar la creación del préstamo si falla la generación de documentos
            }

            // Cargar el préstamo con sus relaciones
            var prestamoCreado = await _context.Prestamos
                .Include(p => p.Persona)
                .Include(p => p.EstadoPrestamo)
                .Include(p => p.FrecuenciaPago)
                .Include(p => p.PrestamoComponentes)
                    .ThenInclude(pc => pc.ComponentePrestamo)
                .Include(p => p.PrestamoGarantias)
                    .ThenInclude(pg => pg.Garantia)
                        .ThenInclude(g => g.TipoGarantia)
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
    // DESHABILITADO: Los préstamos no pueden editarse una vez creados
    [HttpPut("{id}")]
    public IActionResult UpdatePrestamo(long id, Prestamo prestamo)
    {
        return BadRequest(new
        {
            message = "Los préstamos no pueden ser modificados una vez creados por razones de integridad financiera y auditoría.",
            code = "PRESTAMO_NO_EDITABLE"
        });
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
    public async Task<ActionResult<IEnumerable<Prestamo>>> GetPrestamosByPersona(
        long personaId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Prestamos
                .Where(p => p.PersonaId == personaId)
                .Include(p => p.EstadoPrestamo)
                .Include(p => p.FrecuenciaPago)
                .AsQueryable();

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
            _logger.LogError(ex, "Error al obtener los préstamos de la persona {PersonaId}", personaId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/prestamos/activos
    [HttpGet("activos")]
    public async Task<ActionResult<IEnumerable<Prestamo>>> GetPrestamosActivos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var estadoActivo = await _context.EstadosPrestamo
                .Where(e => e.EstadoPrestamoEstaActivo && e.EstadoPrestamoNombre.Contains("Activo"))
                .Select(e => e.EstadoPrestamoId)
                .FirstOrDefaultAsync();

            var query = _context.Prestamos
                .Where(p => p.EstadoPrestamoId == estadoActivo && p.PrestamoSaldoCapital > 0)
                .Include(p => p.Persona)
                .Include(p => p.EstadoPrestamo)
                .AsQueryable();

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
            _logger.LogError(ex, "Error al obtener los préstamos activos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // GET: api/prestamos/buscar?termino=xxx
    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<object>>> BuscarPrestamos([FromQuery] string termino)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return BadRequest(new { message = "El término de búsqueda no puede estar vacío" });
            }

            termino = termino.ToUpper().Trim();

            var prestamos = await _context.Prestamos
                .Include(p => p.Persona)
                .Include(p => p.EstadoPrestamo)
                .Where(p =>
                    p.PrestamoNumero.ToUpper().Contains(termino) ||
                    (p.Persona.PersonaPrimerNombre + " " + p.Persona.PersonaPrimerApellido).ToUpper().Contains(termino) ||
                    (p.Persona.PersonaPrimerNombre + " " + p.Persona.PersonaSegundoNombre + " " + p.Persona.PersonaPrimerApellido + " " + p.Persona.PersonaSegundoApellido).ToUpper().Contains(termino)
                )
                .OrderByDescending(p => p.PrestamoFechaDesembolso)
                .Take(20)
                .Select(p => new
                {
                    prestamoId = p.PrestamoId,
                    prestamoNumero = p.PrestamoNumero,
                    nombreCliente = p.Persona.PersonaPrimerNombre + " " +
                                   (string.IsNullOrEmpty(p.Persona.PersonaSegundoNombre) ? "" : p.Persona.PersonaSegundoNombre + " ") +
                                   p.Persona.PersonaPrimerApellido + " " +
                                   (string.IsNullOrEmpty(p.Persona.PersonaSegundoApellido) ? "" : p.Persona.PersonaSegundoApellido),
                    prestamoMonto = p.PrestamoMonto,
                    prestamoSaldoCapital = p.PrestamoSaldoCapital,
                    prestamoSaldoInteres = p.PrestamoSaldoInteres,
                    prestamoSaldoMora = p.PrestamoSaldoMora,
                    prestamoFechaDesembolso = p.PrestamoFechaDesembolso,
                    estadoPrestamoId = p.EstadoPrestamoId,
                    estadoPrestamoNombre = p.EstadoPrestamo.EstadoPrestamoNombre
                })
                .ToListAsync();

            return Ok(prestamos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar préstamos con término: {Termino}", termino);
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
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

    // GET: api/prestamos/5/documentos/contrato
    [HttpGet("{id}/documentos/contrato")]
    public async Task<IActionResult> GenerarContrato(long id, [FromQuery] string? usuario = "system")
    {
        try
        {
            var html = await _documentoService.GenerarContratoPrestamo(id);

            // Registrar impresión
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _documentoService.RegistrarImpresion(id, "CONTRATO", usuario ?? "system", clientIp);

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar contrato del préstamo {PrestamoId}", id);
            return StatusCode(500, new { message = "Error al generar el contrato", error = ex.Message });
        }
    }

    // GET: api/prestamos/5/documentos/pagare
    [HttpGet("{id}/documentos/pagare")]
    public async Task<IActionResult> GenerarPagare(long id, [FromQuery] string? usuario = "system")
    {
        try
        {
            var html = await _documentoService.GenerarPagare(id);

            // Registrar impresión
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _documentoService.RegistrarImpresion(id, "PAGARE", usuario ?? "system", clientIp);

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar pagaré del préstamo {PrestamoId}", id);
            return StatusCode(500, new { message = "Error al generar el pagaré", error = ex.Message });
        }
    }

    // GET: api/prestamos/5/documentos/plan-pagos
    [HttpGet("{id}/documentos/plan-pagos")]
    public async Task<IActionResult> GenerarPlanPagos(long id, [FromQuery] string? usuario = "system")
    {
        try
        {
            var html = await _documentoService.GenerarPlanPagos(id);

            // Registrar impresión
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _documentoService.RegistrarImpresion(id, "PLAN_PAGOS", usuario ?? "system", clientIp);

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar plan de pagos del préstamo {PrestamoId}", id);
            return StatusCode(500, new { message = "Error al generar el plan de pagos", error = ex.Message });
        }
    }

    // GET: api/prestamos/5/documentos/historial
    [HttpGet("{id}/documentos/historial")]
    public async Task<IActionResult> ObtenerHistorialImpresiones(long id, [FromQuery] string tipoDocumento)
    {
        try
        {
            if (string.IsNullOrEmpty(tipoDocumento))
            {
                return BadRequest(new { message = "Debe especificar el tipo de documento" });
            }

            var historial = await _documentoService.ObtenerHistorialImpresiones(id, tipoDocumento);

            return Ok(new
            {
                prestamoId = id,
                tipoDocumento,
                totalImpresiones = historial.Count,
                impresiones = historial.Select(i => new
                {
                    fecha = i.PrestamoDocumentoImpresionFecha,
                    usuario = i.PrestamoDocumentoImpresionUsuario,
                    ip = i.PrestamoDocumentoImpresionIp,
                    observaciones = i.PrestamoDocumentoImpresionObservaciones
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de impresiones del préstamo {PrestamoId}", id);
            return StatusCode(500, new { message = "Error al obtener el historial", error = ex.Message });
        }
    }

    // GET: api/prestamos/5/documentos/estadisticas
    [HttpGet("{id}/documentos/estadisticas")]
    public async Task<IActionResult> ObtenerEstadisticasDocumentos(long id)
    {
        try
        {
            var documentos = await _context.PrestamosDocumentos
                .Where(pd => pd.PrestamoId == id)
                .Select(pd => new
                {
                    tipo = pd.PrestamoDocumentoTipo,
                    vecesImpreso = pd.PrestamoDocumentoVecesImpreso,
                    primeraImpresion = pd.PrestamoDocumentoFechaPrimeraImpresion,
                    ultimaImpresion = pd.PrestamoDocumentoFechaUltimaImpresion
                })
                .ToListAsync();

            return Ok(new
            {
                prestamoId = id,
                documentos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de documentos del préstamo {PrestamoId}", id);
            return StatusCode(500, new { message = "Error al obtener las estadísticas", error = ex.Message });
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
