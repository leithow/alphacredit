using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AlphaCredit.Api.Services;

/// <summary>
/// Servicio para calcular y generar componentes de mora en préstamos vencidos
/// Este servicio será ejecutado diariamente por un job de base de datos
/// </summary>
public class PrestamoMoraCalculoService
{
    private readonly AlphaCreditDbContext _context;
    private readonly FechaSistemaService _fechaSistemaService;

    private const string COMPONENTE_CAPITAL = "CAPITAL";
    private const string COMPONENTE_INTERES = "INTERES";
    private const string COMPONENTE_MORA = "MORA";
    private const string ESTADO_PENDIENTE = "PENDIENTE";

    public PrestamoMoraCalculoService(
        AlphaCreditDbContext context,
        FechaSistemaService fechaSistemaService)
    {
        _context = context;
        _fechaSistemaService = fechaSistemaService;
    }

    /// <summary>
    /// Calcula y genera componentes de mora para todos los préstamos con cuotas vencidas
    /// Este método debe ejecutarse diariamente
    /// </summary>
    public async Task<int> GenerarComponentesMoraDiariaAsync()
    {
        var fechaActual = await _fechaSistemaService.ObtenerFechaActualSoloFechaAsync();
        var prestamosAfectados = 0;

        // Obtener préstamos activos con componentes vencidos
        var prestamosConMora = await _context.Prestamos
            .Include(p => p.PrestamoComponentes)
            .ThenInclude(pc => pc.ComponentePrestamo)
            .Where(p => p.EstadoPrestamoId != 3 && // No cancelados (ajustar según tu catálogo)
                       p.PrestamoComponentes.Any(pc =>
                           pc.PrestamoComponenteFechaVencimiento.HasValue &&
                           pc.PrestamoComponenteFechaVencimiento.Value < fechaActual &&
                           pc.PrestamoComponenteSaldo > 0))
            .ToListAsync();

        foreach (var prestamo in prestamosConMora)
        {
            var seGeneronMora = await GenerarMoraParaPrestamoAsync(prestamo, fechaActual);
            if (seGeneronMora)
            {
                prestamosAfectados++;
            }
        }

        await _context.SaveChangesAsync();

        return prestamosAfectados;
    }

    /// <summary>
    /// Genera componentes de mora para un préstamo específico
    /// </summary>
    private async Task<bool> GenerarMoraParaPrestamoAsync(Prestamo prestamo, DateTime fechaCalculo)
    {
        var tasaMoraDiaria = await ObtenerTasaMoraDiariaAsync();
        bool seGeneronMora = false;

        // Obtener componentes vencidos (capital e interés)
        var componentesVencidos = prestamo.PrestamoComponentes
            .Where(pc => pc.PrestamoComponenteFechaVencimiento.HasValue &&
                        pc.PrestamoComponenteFechaVencimiento.Value < fechaCalculo &&
                        pc.PrestamoComponenteSaldo > 0 &&
                        (pc.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_CAPITAL ||
                         pc.ComponentePrestamo.ComponentePrestamoTipo == COMPONENTE_INTERES))
            .ToList();

        var componenteMoraId = await ObtenerComponenteMoraIdAsync();
        var estadoPendienteId = await ObtenerEstadoComponenteIdAsync(ESTADO_PENDIENTE);

        foreach (var componente in componentesVencidos)
        {
            var diasVencidos = (fechaCalculo - componente.PrestamoComponenteFechaVencimiento!.Value).Days;

            if (diasVencidos <= 0) continue;

            // Calcular mora del día (solo por hoy, no acumulativa de todos los días)
            var moraDiaria = componente.PrestamoComponenteSaldo * tasaMoraDiaria;

            if (moraDiaria <= 0) continue;

            // Verificar si ya existe un componente de mora para esta cuota en esta fecha
            var moraExistente = await _context.PrestamosComponentes
                .AnyAsync(pc => pc.PrestamoId == prestamo.PrestamoId &&
                               pc.ComponentePrestamoId == componenteMoraId &&
                               pc.PrestamoComponenteNumeroCuota == componente.PrestamoComponenteNumeroCuota &&
                               pc.PrestamoComponenteFechaVencimiento!.Value.Date == fechaCalculo.Date);

            if (!moraExistente)
            {
                // Crear componente de mora para hoy
                var componenteMora = new PrestamoComponente
                {
                    PrestamoId = prestamo.PrestamoId,
                    ComponentePrestamoId = componenteMoraId,
                    EstadoComponenteId = estadoPendienteId,
                    PrestamoComponenteMonto = moraDiaria,
                    PrestamoComponenteSaldo = moraDiaria,
                    PrestamoComponenteFechaVencimiento = fechaCalculo,
                    PrestamoComponenteNumeroCuota = componente.PrestamoComponenteNumeroCuota
                };

                _context.PrestamosComponentes.Add(componenteMora);
                seGeneronMora = true;
            }
        }

        // Actualizar saldo de mora del préstamo
        if (seGeneronMora)
        {
            await ActualizarSaldoMoraPrestamoAsync(prestamo.PrestamoId);
        }

        return seGeneronMora;
    }

    /// <summary>
    /// Calcula la mora total acumulada de un préstamo
    /// (suma de todos los componentes de mora pendientes)
    /// </summary>
    public async Task<decimal> CalcularMoraTotalPrestamoAsync(long prestamoId)
    {
        var componenteMoraId = await ObtenerComponenteMoraIdAsync();

        var moraTotal = await _context.PrestamosComponentes
            .Where(pc => pc.PrestamoId == prestamoId &&
                        pc.ComponentePrestamoId == componenteMoraId &&
                        pc.PrestamoComponenteSaldo > 0)
            .SumAsync(pc => pc.PrestamoComponenteSaldo);

        return moraTotal;
    }

    /// <summary>
    /// Actualiza el saldo de mora en el préstamo
    /// </summary>
    private async Task ActualizarSaldoMoraPrestamoAsync(long prestamoId)
    {
        var prestamo = await _context.Prestamos.FindAsync(prestamoId);
        if (prestamo == null) return;

        var moraTotal = await CalcularMoraTotalPrestamoAsync(prestamoId);
        prestamo.PrestamoSaldoMora = moraTotal;
        prestamo.PrestamoFechaModifica = await _fechaSistemaService.ObtenerFechaActualAsync();
    }

    /// <summary>
    /// Obtiene el ID del componente de tipo MORA
    /// </summary>
    private async Task<long> ObtenerComponenteMoraIdAsync()
    {
        var componenteMora = await _context.ComponentesPrestamo
            .FirstOrDefaultAsync(c => c.ComponentePrestamoTipo == COMPONENTE_MORA &&
                                     c.ComponentePrestamoEstaActivo);

        if (componenteMora == null)
        {
            throw new InvalidOperationException("No se encontró el componente de tipo MORA. Debe crearlo en el catálogo.");
        }

        return componenteMora.ComponentePrestamoId;
    }

    /// <summary>
    /// Obtiene la tasa de mora diaria desde parámetros del sistema
    /// 3% mensual / 30 días = 0.1% diario
    /// </summary>
    private async Task<decimal> ObtenerTasaMoraDiariaAsync()
    {
        var parametroMora = await _context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.ParametroSistemaLlave == "TASA_MORA_MENSUAL");

        if (parametroMora != null && decimal.TryParse(parametroMora.ParametrosSistemaValor, out var tasaMensual))
        {
            // Convertir tasa mensual a diaria: tasaMensual / 30 / 100
            return tasaMensual / 30 / 100;
        }

        // Valor por defecto: 3% mensual = 0.1% diario
        return 3.0m / 30 / 100;
    }

    /// <summary>
    /// Obtiene el ID de un estado de componente por nombre
    /// </summary>
    private async Task<long> ObtenerEstadoComponenteIdAsync(string nombreEstado)
    {
        var estado = await _context.EstadosComponente
            .FirstOrDefaultAsync(e => e.EstadoComponenteNombre.ToUpper() == nombreEstado.ToUpper());

        if (estado == null)
            throw new InvalidOperationException($"Estado de componente '{nombreEstado}' no encontrado");

        return estado.EstadoComponenteId;
    }
}
