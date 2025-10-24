using AlphaCredit.Api.Data;
using AlphaCredit.Api.DTOs;
using AlphaCredit.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AlphaCredit.Api.Services;

/// <summary>
/// Servicio para aplicar abonos a préstamos
/// Soporta: cuotas completas, parciales, abonos a capital, abonos a mora
/// </summary>
public class PrestamoAbonoService
{
    private readonly AlphaCreditDbContext _context;
    private readonly PrestamoMoraCalculoService _moraCalculoService;
    private readonly FechaSistemaService _fechaSistemaService;

    // IDs de componentes (deberían venir de configuración o catálogo)
    private const string COMPONENTE_CAPITAL = "CAPITAL";
    private const string COMPONENTE_INTERES = "INTERES";
    private const string COMPONENTE_MORA = "MORA";
    private const string ESTADO_PAGADO = "PAGADO";
    private const string ESTADO_PARCIAL = "PARCIAL";
    private const string ESTADO_PENDIENTE = "PENDIENTE";

    public PrestamoAbonoService(
        AlphaCreditDbContext context,
        PrestamoMoraCalculoService moraCalculoService,
        FechaSistemaService fechaSistemaService)
    {
        _context = context;
        _moraCalculoService = moraCalculoService;
        _fechaSistemaService = fechaSistemaService;
    }

    /// <summary>
    /// Aplica un abono a un préstamo según el tipo especificado
    /// </summary>
    public async Task<AbonoPrestamoResponse> AplicarAbonoAsync(AbonoPrestamoRequest request, string usuarioCreador)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validar y cargar préstamo
            var prestamo = await CargarPrestamoCompletoAsync(request.PrestamoId);

            // Usar fecha del sistema si no se especifica
            var fechaPago = request.FechaPago ?? await _fechaSistemaService.ObtenerFechaActualAsync();

            // Actualizar mora del préstamo (suma de componentes MORA pendientes)
            prestamo.PrestamoSaldoMora = await _moraCalculoService.CalcularMoraTotalPrestamoAsync(request.PrestamoId);

            AbonoPrestamoResponse response;

            // Aplicar según tipo de abono
            response = request.TipoAbono.ToUpper() switch
            {
                "CUOTA" => await AplicarAbonoCuotaCompletaAsync(prestamo, request, fechaPago, usuarioCreador),
                "PARCIAL" => await AplicarAbonoParcialAsync(prestamo, request, fechaPago, usuarioCreador),
                "CAPITAL" => await AplicarAbonoCapitalAsync(prestamo, request, fechaPago, usuarioCreador),
                "MORA" => await AplicarAbonoMoraAsync(prestamo, request, fechaPago, usuarioCreador),
                _ => throw new InvalidOperationException($"Tipo de abono '{request.TipoAbono}' no válido")
            };

            // Actualizar saldos del préstamo
            await ActualizarSaldosPrestamoAsync(prestamo.PrestamoId);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return response;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Aplica abono a cuota completa (paga cuotas en orden de vencimiento)
    /// </summary>
    private async Task<AbonoPrestamoResponse> AplicarAbonoCuotaCompletaAsync(
        Prestamo prestamo,
        AbonoPrestamoRequest request,
        DateTime fechaPago,
        string usuarioCreador)
    {
        var response = new AbonoPrestamoResponse
        {
            FechaAplicacion = fechaPago,
            MontoAplicado = 0
        };

        decimal montoRestante = request.Monto;
        var componentesAfectados = new List<ComponenteAfectadoDto>();

        // Obtener cuotas pendientes ordenadas por fecha de vencimiento (vencidas primero)
        var cuotasPendientes = await ObtenerCuotasPendientesOrdenadasAsync(prestamo.PrestamoId, fechaPago);

        foreach (var cuota in cuotasPendientes)
        {
            if (montoRestante <= 0) break;

            decimal totalCuota = cuota.Sum(c => c.PrestamoComponenteSaldo);

            if (montoRestante >= totalCuota)
            {
                // Pagar cuota completa
                foreach (var componente in cuota)
                {
                    var montoPagado = componente.PrestamoComponenteSaldo;

                    componentesAfectados.Add(await AplicarPagoComponenteAsync(
                        componente,
                        montoPagado,
                        fechaPago));

                    montoRestante -= montoPagado;

                    ActualizarMontosPorTipo(response, componente.ComponentePrestamo.ComponentePrestamoTipo, montoPagado);
                }
            }
            else
            {
                // Abono parcial a esta cuota
                montoRestante = await AplicarAbonoParcialCuotaAsync(
                    cuota,
                    montoRestante,
                    fechaPago,
                    componentesAfectados,
                    response);
                break;
            }
        }

        // Crear movimiento de préstamo
        var movimiento = await CrearMovimientoPrestamoAsync(
            prestamo.PrestamoId,
            request.FormaPagoId,
            "PAGO",
            fechaPago,
            response.MontoCapital,
            response.MontoInteres,
            response.MontoMora,
            0,
            request.Observaciones,
            usuarioCreador);

        response.MovimientoPrestamoId = movimiento.MovimientoPrestamoId;
        response.MontoAplicado = request.Monto - montoRestante;
        response.ComponentesAfectados = componentesAfectados;
        response.Mensaje = $"Abono aplicado a {cuotasPendientes.Count(c => c.All(comp => comp.PrestamoComponenteSaldo == 0))} cuota(s) completa(s)";

        // Crear registros de pago detalle
        await CrearPagosDetalleAsync(movimiento.MovimientoPrestamoId, componentesAfectados, fechaPago);

        return response;
    }

    /// <summary>
    /// Aplica abono parcial distribuyendo el monto
    /// </summary>
    private async Task<AbonoPrestamoResponse> AplicarAbonoParcialAsync(
        Prestamo prestamo,
        AbonoPrestamoRequest request,
        DateTime fechaPago,
        string usuarioCreador)
    {
        var response = new AbonoPrestamoResponse
        {
            FechaAplicacion = fechaPago
        };

        var componentesAfectados = new List<ComponenteAfectadoDto>();

        // Si hay distribución manual
        if (request.Distribucion != null)
        {
            await AplicarDistribucionManualAsync(
                prestamo,
                request.Distribucion,
                request.NumeroCuota,
                fechaPago,
                componentesAfectados,
                response);
        }
        else
        {
            // Distribución automática: Mora > Interés > Capital (orden de prioridad)
            await AplicarDistribucionAutomaticaAsync(
                prestamo,
                request.Monto,
                request.NumeroCuota,
                fechaPago,
                componentesAfectados,
                response);
        }

        // Crear movimiento
        var movimiento = await CrearMovimientoPrestamoAsync(
            prestamo.PrestamoId,
            request.FormaPagoId,
            "PAGO_PARCIAL",
            fechaPago,
            response.MontoCapital,
            response.MontoInteres,
            response.MontoMora,
            response.MontoOtros,
            request.Observaciones,
            usuarioCreador);

        response.MovimientoPrestamoId = movimiento.MovimientoPrestamoId;
        response.MontoAplicado = response.MontoCapital + response.MontoInteres + response.MontoMora + response.MontoOtros;
        response.ComponentesAfectados = componentesAfectados;
        response.Mensaje = "Abono parcial aplicado correctamente";

        // Crear registros de pago detalle
        await CrearPagosDetalleAsync(movimiento.MovimientoPrestamoId, componentesAfectados, fechaPago);

        return response;
    }

    /// <summary>
    /// Aplica abono directo a capital (reduce saldo sin afectar intereses)
    /// </summary>
    private async Task<AbonoPrestamoResponse> AplicarAbonoCapitalAsync(
        Prestamo prestamo,
        AbonoPrestamoRequest request,
        DateTime fechaPago,
        string usuarioCreador)
    {
        var response = new AbonoPrestamoResponse
        {
            FechaAplicacion = fechaPago,
            MontoCapital = request.Monto
        };

        var componentesAfectados = new List<ComponenteAfectadoDto>();
        decimal montoRestante = request.Monto;

        // Obtener componentes de capital pendientes
        var componentesCapital = await _context.PrestamosComponentes
            .Include(pc => pc.ComponentePrestamo)
            .Where(pc => pc.PrestamoId == prestamo.PrestamoId &&
                        pc.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_CAPITAL &&
                        pc.PrestamoComponenteSaldo > 0)
            .OrderBy(pc => pc.PrestamoComponenteNumeroCuota)
            .ToListAsync();

        foreach (var componente in componentesCapital)
        {
            if (montoRestante <= 0) break;

            decimal montoAplicar = Math.Min(montoRestante, componente.PrestamoComponenteSaldo);

            componentesAfectados.Add(await AplicarPagoComponenteAsync(
                componente,
                montoAplicar,
                fechaPago));

            montoRestante -= montoAplicar;
        }

        // Crear movimiento
        var movimiento = await CrearMovimientoPrestamoAsync(
            prestamo.PrestamoId,
            request.FormaPagoId,
            "ABONO_CAPITAL",
            fechaPago,
            response.MontoCapital,
            0,
            0,
            0,
            request.Observaciones ?? "Abono extraordinario a capital",
            usuarioCreador);

        response.MovimientoPrestamoId = movimiento.MovimientoPrestamoId;
        response.MontoAplicado = request.Monto - montoRestante;
        response.ComponentesAfectados = componentesAfectados;
        response.Mensaje = $"Abono a capital aplicado. Monto: {response.MontoAplicado:C}";

        // Crear registros de pago detalle
        await CrearPagosDetalleAsync(movimiento.MovimientoPrestamoId, componentesAfectados, fechaPago);

        return response;
    }

    /// <summary>
    /// Aplica abono específico a mora
    /// </summary>
    private async Task<AbonoPrestamoResponse> AplicarAbonoMoraAsync(
        Prestamo prestamo,
        AbonoPrestamoRequest request,
        DateTime fechaPago,
        string usuarioCreador)
    {
        var response = new AbonoPrestamoResponse
        {
            FechaAplicacion = fechaPago
        };

        var componentesAfectados = new List<ComponenteAfectadoDto>();
        decimal montoRestante = request.Monto;

        // Obtener todos los componentes de MORA pendientes ordenados por fecha
        var componentesMora = await _context.PrestamosComponentes
            .Include(pc => pc.ComponentePrestamo)
            .Include(pc => pc.EstadoComponente)
            .Where(pc => pc.PrestamoId == prestamo.PrestamoId &&
                        pc.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_MORA &&
                        pc.PrestamoComponenteSaldo > 0)
            .OrderBy(pc => pc.PrestamoComponenteFechaVencimiento)
            .ThenBy(pc => pc.PrestamoComponenteNumeroCuota)
            .ToListAsync();

        // Aplicar pago a cada componente de mora
        foreach (var componente in componentesMora)
        {
            if (montoRestante <= 0) break;

            decimal montoAplicar = Math.Min(montoRestante, componente.PrestamoComponenteSaldo);

            componentesAfectados.Add(await AplicarPagoComponenteAsync(
                componente,
                montoAplicar,
                fechaPago));

            montoRestante -= montoAplicar;
            response.MontoMora += montoAplicar;
        }

        // Actualizar saldo de mora del préstamo
        prestamo.PrestamoSaldoMora = await _moraCalculoService.CalcularMoraTotalPrestamoAsync(prestamo.PrestamoId);

        // Crear movimiento
        var movimiento = await CrearMovimientoPrestamoAsync(
            prestamo.PrestamoId,
            request.FormaPagoId,
            "PAGO_MORA",
            fechaPago,
            0,
            0,
            response.MontoMora,
            0,
            request.Observaciones ?? "Pago de mora",
            usuarioCreador);

        response.MovimientoPrestamoId = movimiento.MovimientoPrestamoId;
        response.MontoAplicado = response.MontoMora;
        response.ComponentesAfectados = componentesAfectados;
        response.Mensaje = $"Pago de mora aplicado. Monto: {response.MontoMora:C}";

        // Crear registros de pago detalle
        await CrearPagosDetalleAsync(movimiento.MovimientoPrestamoId, componentesAfectados, fechaPago);

        return response;
    }

    // ========== MÉTODOS AUXILIARES ==========

    private async Task<Prestamo> CargarPrestamoCompletoAsync(long prestamoId)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.PrestamoComponentes)
            .ThenInclude(pc => pc.ComponentePrestamo)
            .Include(p => p.PrestamoComponentes)
            .ThenInclude(pc => pc.EstadoComponente)
            .FirstOrDefaultAsync(p => p.PrestamoId == prestamoId);

        if (prestamo == null)
            throw new InvalidOperationException($"Préstamo {prestamoId} no encontrado");

        return prestamo;
    }

    private async Task<List<List<PrestamoComponente>>> ObtenerCuotasPendientesOrdenadasAsync(
        long prestamoId,
        DateTime fechaReferencia)
    {
        var componentes = await _context.PrestamosComponentes
            .Include(pc => pc.ComponentePrestamo)
            .Include(pc => pc.EstadoComponente)
            .Where(pc => pc.PrestamoId == prestamoId &&
                        pc.PrestamoComponenteSaldo > 0)
            .OrderBy(pc => pc.PrestamoComponenteFechaVencimiento)
            .ThenBy(pc => pc.PrestamoComponenteNumeroCuota)
            .ToListAsync();

        // Agrupar por número de cuota
        return componentes
            .GroupBy(pc => pc.PrestamoComponenteNumeroCuota)
            .Select(g => g.OrderBy(c => c.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_MORA ? 0 :
                                       c.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_INTERES ? 1 : 2)
                         .ToList())
            .ToList();
    }

    private async Task<decimal> AplicarAbonoParcialCuotaAsync(
        List<PrestamoComponente> cuota,
        decimal montoDisponible,
        DateTime fechaPago,
        List<ComponenteAfectadoDto> componentesAfectados,
        AbonoPrestamoResponse response)
    {
        // ORDEN CRÍTICO: Mora PRIMERO, luego Interés, luego Capital
        var ordenados = cuota.OrderBy(c =>
            c.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_MORA ? 0 :
            c.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_INTERES ? 1 : 2).ToList();

        foreach (var componente in ordenados)
        {
            if (montoDisponible <= 0) break;

            decimal montoAplicar = Math.Min(montoDisponible, componente.PrestamoComponenteSaldo);

            componentesAfectados.Add(await AplicarPagoComponenteAsync(
                componente,
                montoAplicar,
                fechaPago));

            montoDisponible -= montoAplicar;
            ActualizarMontosPorTipo(response, componente.ComponentePrestamo.ComponentePrestamoTipo, montoAplicar);
        }

        return montoDisponible;
    }

    private async Task AplicarDistribucionAutomaticaAsync(
        Prestamo prestamo,
        decimal monto,
        int? numeroCuota,
        DateTime fechaPago,
        List<ComponenteAfectadoDto> componentesAfectados,
        AbonoPrestamoResponse response)
    {
        decimal montoRestante = monto;

        // 1. Pagar MORA primero (componentes de tipo MORA)
        if (montoRestante > 0)
        {
            var componentesMora = await ObtenerComponentesPorTipoAsync(
                prestamo.PrestamoId,
                COMPONENTE_MORA,
                numeroCuota);

            foreach (var componente in componentesMora)
            {
                if (montoRestante <= 0) break;

                decimal montoAplicar = Math.Min(montoRestante, componente.PrestamoComponenteSaldo);
                componentesAfectados.Add(await AplicarPagoComponenteAsync(componente, montoAplicar, fechaPago));

                response.MontoMora += montoAplicar;
                montoRestante -= montoAplicar;
            }

            // Actualizar saldo de mora del préstamo
            prestamo.PrestamoSaldoMora = await _moraCalculoService.CalcularMoraTotalPrestamoAsync(prestamo.PrestamoId);
        }

        // 2. Pagar interés
        if (montoRestante > 0)
        {
            var componentesInteres = await ObtenerComponentesPorTipoAsync(
                prestamo.PrestamoId,
                COMPONENTE_INTERES,
                numeroCuota);

            foreach (var componente in componentesInteres)
            {
                if (montoRestante <= 0) break;

                decimal montoAplicar = Math.Min(montoRestante, componente.PrestamoComponenteSaldo);
                componentesAfectados.Add(await AplicarPagoComponenteAsync(componente, montoAplicar, fechaPago));

                response.MontoInteres += montoAplicar;
                montoRestante -= montoAplicar;
            }
        }

        // 3. Pagar capital
        if (montoRestante > 0)
        {
            var componentesCapital = await ObtenerComponentesPorTipoAsync(
                prestamo.PrestamoId,
                COMPONENTE_CAPITAL,
                numeroCuota);

            foreach (var componente in componentesCapital)
            {
                if (montoRestante <= 0) break;

                decimal montoAplicar = Math.Min(montoRestante, componente.PrestamoComponenteSaldo);
                componentesAfectados.Add(await AplicarPagoComponenteAsync(componente, montoAplicar, fechaPago));

                response.MontoCapital += montoAplicar;
                montoRestante -= montoAplicar;
            }
        }
    }

    private async Task AplicarDistribucionManualAsync(
        Prestamo prestamo,
        DistribucionAbonoDto distribucion,
        int? numeroCuota,
        DateTime fechaPago,
        List<ComponenteAfectadoDto> componentesAfectados,
        AbonoPrestamoResponse response)
    {
        // Aplicar mora a componentes MORA
        if (distribucion.MontoMora.HasValue && distribucion.MontoMora.Value > 0)
        {
            response.MontoMora += await AplicarMontoATipoComponenteAsync(
                prestamo.PrestamoId,
                COMPONENTE_MORA,
                distribucion.MontoMora.Value,
                numeroCuota,
                fechaPago,
                componentesAfectados);

            // Actualizar saldo de mora del préstamo
            prestamo.PrestamoSaldoMora = await _moraCalculoService.CalcularMoraTotalPrestamoAsync(prestamo.PrestamoId);
        }

        // Aplicar interés
        if (distribucion.MontoInteres.HasValue && distribucion.MontoInteres.Value > 0)
        {
            response.MontoInteres += await AplicarMontoATipoComponenteAsync(
                prestamo.PrestamoId,
                COMPONENTE_INTERES,
                distribucion.MontoInteres.Value,
                numeroCuota,
                fechaPago,
                componentesAfectados);
        }

        // Aplicar capital
        if (distribucion.MontoCapital.HasValue && distribucion.MontoCapital.Value > 0)
        {
            response.MontoCapital += await AplicarMontoATipoComponenteAsync(
                prestamo.PrestamoId,
                COMPONENTE_CAPITAL,
                distribucion.MontoCapital.Value,
                numeroCuota,
                fechaPago,
                componentesAfectados);
        }

        // Otros cargos
        if (distribucion.MontoOtros.HasValue && distribucion.MontoOtros.Value > 0)
        {
            response.MontoOtros = distribucion.MontoOtros.Value;
        }
    }

    private async Task<decimal> AplicarMontoATipoComponenteAsync(
        long prestamoId,
        string tipoComponente,
        decimal monto,
        int? numeroCuota,
        DateTime fechaPago,
        List<ComponenteAfectadoDto> componentesAfectados)
    {
        var componentes = await ObtenerComponentesPorTipoAsync(prestamoId, tipoComponente, numeroCuota);
        decimal montoRestante = monto;
        decimal montoAcumulado = 0;

        foreach (var componente in componentes)
        {
            if (montoRestante <= 0) break;

            decimal montoAplicar = Math.Min(montoRestante, componente.PrestamoComponenteSaldo);
            componentesAfectados.Add(await AplicarPagoComponenteAsync(componente, montoAplicar, fechaPago));

            montoAcumulado += montoAplicar;
            montoRestante -= montoAplicar;
        }

        return montoAcumulado;
    }

    private async Task<List<PrestamoComponente>> ObtenerComponentesPorTipoAsync(
        long prestamoId,
        string tipoComponente,
        int? numeroCuota)
    {
        var query = _context.PrestamosComponentes
            .Include(pc => pc.ComponentePrestamo)
            .Include(pc => pc.EstadoComponente)
            .Where(pc => pc.PrestamoId == prestamoId &&
                        pc.ComponentePrestamo.ComponentePrestamoTipo == tipoComponente &&
                        pc.PrestamoComponenteSaldo > 0);

        if (numeroCuota.HasValue)
        {
            query = query.Where(pc => pc.PrestamoComponenteNumeroCuota == numeroCuota.Value);
        }

        return await query
            .OrderBy(pc => pc.PrestamoComponenteFechaVencimiento)
            .ThenBy(pc => pc.PrestamoComponenteNumeroCuota)
            .ToListAsync();
    }

    private async Task<ComponenteAfectadoDto> AplicarPagoComponenteAsync(
        PrestamoComponente componente,
        decimal montoPago,
        DateTime fechaPago)
    {
        var montoAntes = componente.PrestamoComponenteSaldo;
        componente.PrestamoComponenteSaldo -= montoPago;

        // Actualizar estado del componente
        var estadoPagadoId = await ObtenerEstadoComponenteIdAsync(
            componente.PrestamoComponenteSaldo == 0 ? ESTADO_PAGADO :
            componente.PrestamoComponenteSaldo < componente.PrestamoComponenteMonto ? ESTADO_PARCIAL :
            ESTADO_PENDIENTE);

        componente.EstadoComponenteId = estadoPagadoId;

        // NO crear pagodetalle aquí - se creará después cuando tengamos el MovimientoPrestamoId

        return new ComponenteAfectadoDto
        {
            PrestamoComponenteId = componente.PrestamoComponenteId,
            ComponenteNombre = componente.ComponentePrestamo.ComponentePrestamoNombre,
            NumeroCuota = componente.PrestamoComponenteNumeroCuota,
            MontoAntes = montoAntes,
            MontoAplicado = montoPago,
            SaldoNuevo = componente.PrestamoComponenteSaldo,
            EstadoNuevo = componente.PrestamoComponenteSaldo == 0 ? "Pagado" : "Pendiente"
        };
    }

    private async Task<MovimientoPrestamo> CrearMovimientoPrestamoAsync(
        long prestamoId,
        long? formaPagoId,
        string tipoMovimiento,
        DateTime fecha,
        decimal montoCapital,
        decimal montoInteres,
        decimal montoMora,
        decimal montoOtros,
        string? observaciones,
        string usuarioCreador)
    {
        var montoTotal = montoCapital + montoInteres + montoMora + montoOtros;

        var movimiento = new MovimientoPrestamo
        {
            PrestamoId = prestamoId,
            FormaPagoId = formaPagoId,
            MovimientoPrestamoTipo = tipoMovimiento,
            MovimientoPrestamoFecha = fecha,
            MovimientoPrestamoMontoCapital = montoCapital,
            MovimientoPrestamoMontoInteres = montoInteres,
            MovimientoPrestamoMontoMora = montoMora,
            MovimientoPrestamoMontoOtros = montoOtros,
            MovimientoPrestamoMontoTotal = montoTotal,
            MovimientoPrestamoObservaciones = observaciones,
            MovimientoPrestamoUserCrea = usuarioCreador,
            MovimientoPrestamoFechaCreacion = DateTime.UtcNow
        };

        _context.MovimientosPrestamo.Add(movimiento);
        await _context.SaveChangesAsync(); // Guardar para obtener el ID

        // Registrar movimiento en el fondo si la forma de pago tiene fondo asociado
        if (formaPagoId.HasValue)
        {
            await RegistrarMovimientoFondoAsync(
                formaPagoId.Value,
                montoTotal,
                fecha,
                $"Abono a préstamo - {tipoMovimiento}",
                observaciones,
                usuarioCreador);
        }

        return movimiento;
    }

    private async Task CrearPagosDetalleAsync(
        long movimientoPrestamoId,
        List<ComponenteAfectadoDto> componentesAfectados,
        DateTime fechaPago)
    {
        foreach (var componente in componentesAfectados)
        {
            if (componente.MontoAplicado > 0 && componente.PrestamoComponenteId.HasValue)
            {
                var pagoDetalle = new PagoDetalle
                {
                    MovimientoPrestamoId = movimientoPrestamoId,
                    PrestamoComponenteId = componente.PrestamoComponenteId.Value,
                    ComponentePrestamoId = await ObtenerComponentePrestamoIdPorNombreAsync(componente.ComponenteNombre),
                    PagoDetalleCuotaNumero = componente.NumeroCuota,
                    PagoDetalleMontoAplicado = componente.MontoAplicado,
                    PagoDetalleMontoAntes = componente.MontoAntes,
                    PagoDetalleFechaAplicacion = fechaPago
                };

                _context.Add(pagoDetalle);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<long> ObtenerComponentePrestamoIdPorNombreAsync(string nombre)
    {
        var componente = await _context.ComponentesPrestamo
            .FirstOrDefaultAsync(c => c.ComponentePrestamoNombre == nombre);

        return componente?.ComponentePrestamoId ??
               throw new InvalidOperationException($"Componente de préstamo '{nombre}' no encontrado");
    }

    /// <summary>
    /// Registra un ingreso en el fondo asociado a la forma de pago
    /// </summary>
    private async Task RegistrarMovimientoFondoAsync(
        long formaPagoId,
        decimal monto,
        DateTime fecha,
        string concepto,
        string? observacionesAdicionales,
        string usuarioCreador)
    {
        // Obtener la forma de pago con su fondo asociado
        var formaPago = await _context.FormasPago
            .Include(fp => fp.Fondo)
            .FirstOrDefaultAsync(fp => fp.FormaPagoId == formaPagoId);

        if (formaPago?.FondoId == null)
        {
            // Si no tiene fondo asociado, no hacer nada
            return;
        }

        // Crear movimiento de ingreso en el fondo
        var fondoMovimiento = new FondoMovimiento
        {
            FondoId = formaPago.FondoId.Value,
            FondoMovimientoTipo = "INGRESO",
            FondoMovimientoFecha = fecha,
            FondoMovimientoMonto = monto,
            FondoMovimientoConcepto = observacionesAdicionales != null
                ? $"{concepto} - {observacionesAdicionales}"
                : concepto,
            FondoMovimientoUserCrea = usuarioCreador,
            FondoMovimientoFechaCreacion = DateTime.UtcNow
        };

        _context.FondosMovimientos.Add(fondoMovimiento);

        // Actualizar saldo del fondo
        var fondo = await _context.Fondos.FindAsync(formaPago.FondoId.Value);
        if (fondo != null)
        {
            fondo.FondoSaldoActual += monto;
            fondo.FondoFechaModifica = DateTime.UtcNow;
            fondo.FondoUserModifica = usuarioCreador;
        }

        await _context.SaveChangesAsync();
    }

    private async Task ActualizarSaldosPrestamoAsync(long prestamoId)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.PrestamoComponentes)
            .ThenInclude(pc => pc.ComponentePrestamo)
            .FirstOrDefaultAsync(p => p.PrestamoId == prestamoId);

        if (prestamo == null) return;

        prestamo.PrestamoSaldoCapital = prestamo.PrestamoComponentes
            .Where(pc => pc.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_CAPITAL)
            .Sum(pc => pc.PrestamoComponenteSaldo);

        prestamo.PrestamoSaldoInteres = prestamo.PrestamoComponentes
            .Where(pc => pc.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_INTERES)
            .Sum(pc => pc.PrestamoComponenteSaldo);

        prestamo.PrestamoFechaModifica = DateTime.UtcNow;
    }

    private async Task<long> ObtenerEstadoComponenteIdAsync(string nombreEstado)
    {
        var estado = await _context.EstadosComponente
            .FirstOrDefaultAsync(e => e.EstadoComponenteNombre.ToUpper() == nombreEstado.ToUpper());

        if (estado == null)
            throw new InvalidOperationException($"Estado de componente '{nombreEstado}' no encontrado");

        return estado.EstadoComponenteId;
    }

    private void ActualizarMontosPorTipo(AbonoPrestamoResponse response, string? tipoComponente, decimal monto)
    {
        switch (tipoComponente?.ToUpper())
        {
            case COMPONENTE_CAPITAL:
                response.MontoCapital += monto;
                break;
            case COMPONENTE_INTERES:
                response.MontoInteres += monto;
                break;
            case COMPONENTE_MORA:
                response.MontoMora += monto;
                break;
            default:
                response.MontoOtros += monto;
                break;
        }
    }
}
