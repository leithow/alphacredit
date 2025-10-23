using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AlphaCredit.Api.Services;

/// <summary>
/// Servicio para cálculo de mora en préstamos
/// </summary>
public class PrestamoMoraService
{
    private readonly AlphaCreditDbContext _context;

    public PrestamoMoraService(AlphaCreditDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Calcula la mora para un préstamo a una fecha específica
    /// </summary>
    public async Task<decimal> CalcularMoraPrestamoAsync(long prestamoId, DateTime fechaCalculo)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.PrestamoComponentes)
            .ThenInclude(pc => pc.ComponentePrestamo)
            .FirstOrDefaultAsync(p => p.PrestamoId == prestamoId);

        if (prestamo == null)
            throw new InvalidOperationException("Préstamo no encontrado");

        // Obtener tasa de mora del sistema (puedes ajustar esto según tus parámetros)
        var tasaMoraDiaria = await ObtenerTasaMoraDiariaAsync();

        decimal moraTotal = 0;

        // Calcular mora solo para componentes vencidos con saldo pendiente
        var componentesVencidos = prestamo.PrestamoComponentes
            .Where(pc => pc.PrestamoComponenteFechaVencimiento.HasValue &&
                        pc.PrestamoComponenteFechaVencimiento.Value < fechaCalculo &&
                        pc.PrestamoComponenteSaldo > 0)
            .ToList();

        foreach (var componente in componentesVencidos)
        {
            var diasVencidos = (fechaCalculo - componente.PrestamoComponenteFechaVencimiento!.Value).Days;

            if (diasVencidos > 0)
            {
                // Mora = Saldo pendiente * Tasa mora diaria * Días vencidos
                var moraComponente = componente.PrestamoComponenteSaldo * tasaMoraDiaria * diasVencidos;
                moraTotal += moraComponente;
            }
        }

        return Math.Round(moraTotal, 2);
    }

    /// <summary>
    /// Calcula la mora para componentes específicos
    /// </summary>
    public async Task<Dictionary<long, decimal>> CalcularMoraPorComponenteAsync(
        List<PrestamoComponente> componentes,
        DateTime fechaCalculo)
    {
        var tasaMoraDiaria = await ObtenerTasaMoraDiariaAsync();
        var moraPorComponente = new Dictionary<long, decimal>();

        foreach (var componente in componentes)
        {
            if (componente.PrestamoComponenteFechaVencimiento.HasValue &&
                componente.PrestamoComponenteFechaVencimiento.Value < fechaCalculo &&
                componente.PrestamoComponenteSaldo > 0)
            {
                var diasVencidos = (fechaCalculo - componente.PrestamoComponenteFechaVencimiento.Value).Days;

                if (diasVencidos > 0)
                {
                    var moraComponente = componente.PrestamoComponenteSaldo * tasaMoraDiaria * diasVencidos;
                    moraPorComponente[componente.PrestamoComponenteId] = Math.Round(moraComponente, 2);
                }
                else
                {
                    moraPorComponente[componente.PrestamoComponenteId] = 0;
                }
            }
            else
            {
                moraPorComponente[componente.PrestamoComponenteId] = 0;
            }
        }

        return moraPorComponente;
    }

    /// <summary>
    /// Obtiene componentes vencidos ordenados por fecha de vencimiento
    /// </summary>
    public async Task<List<PrestamoComponente>> ObtenerComponentesVencidosAsync(
        long prestamoId,
        DateTime fechaCalculo)
    {
        return await _context.PrestamosComponentes
            .Include(pc => pc.ComponentePrestamo)
            .Include(pc => pc.EstadoComponente)
            .Where(pc => pc.PrestamoId == prestamoId &&
                        pc.PrestamoComponenteFechaVencimiento.HasValue &&
                        pc.PrestamoComponenteFechaVencimiento.Value < fechaCalculo &&
                        pc.PrestamoComponenteSaldo > 0)
            .OrderBy(pc => pc.PrestamoComponenteFechaVencimiento)
            .ThenBy(pc => pc.PrestamoComponenteNumeroCuota)
            .ThenBy(pc => pc.ComponentePrestamo.ComponentePrestamoTipo) // Mora primero, luego interés, luego capital
            .ToListAsync();
    }

    /// <summary>
    /// Calcula días de mora para un componente
    /// </summary>
    public int CalcularDiasMora(PrestamoComponente componente, DateTime fechaCalculo)
    {
        if (!componente.PrestamoComponenteFechaVencimiento.HasValue)
            return 0;

        if (componente.PrestamoComponenteFechaVencimiento.Value >= fechaCalculo)
            return 0;

        return (fechaCalculo - componente.PrestamoComponenteFechaVencimiento.Value).Days;
    }

    /// <summary>
    /// Obtiene la tasa de mora diaria desde parámetros del sistema
    /// 3% mensual / 30 días = 0.1% diario
    /// </summary>
    private async Task<decimal> ObtenerTasaMoraDiariaAsync()
    {
        // Buscar en parámetros del sistema
        var parametroMora = await _context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.ParametroSistemaLlave == "TASA_MORA_MENSUAL");

        if (parametroMora != null && decimal.TryParse(parametroMora.ParametrosSistemaValor, out var tasaMensual))
        {
            // Convertir tasa mensual a diaria: 3% / 30 días = 0.1% diario
            return tasaMensual / 30 / 100;
        }

        // Valor por defecto: 3% mensual = 0.1% diario
        return 3.0m / 30 / 100;
    }

    /// <summary>
    /// Actualiza el saldo de mora en el préstamo
    /// </summary>
    public async Task ActualizarSaldoMoraPrestamoAsync(long prestamoId, DateTime fechaCalculo)
    {
        var prestamo = await _context.Prestamos.FindAsync(prestamoId);
        if (prestamo == null)
            throw new InvalidOperationException("Préstamo no encontrado");

        var moraCalculada = await CalcularMoraPrestamoAsync(prestamoId, fechaCalculo);
        prestamo.PrestamoSaldoMora = moraCalculada;

        await _context.SaveChangesAsync();
    }
}
