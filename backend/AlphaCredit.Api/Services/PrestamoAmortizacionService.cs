using AlphaCredit.Api.Models;

namespace AlphaCredit.Api.Services;

public class PrestamoAmortizacionService
{
    /// <summary>
    /// Genera los componentes de capital e interés para un préstamo
    /// </summary>
    public List<PrestamoComponente> GenerarComponentes(
        long prestamoId,
        decimal montoPrestamo,
        decimal tasaInteresAnual,
        int numeroCuotas,
        short frecuenciaPagoDias,
        DateTime fechaDesembolso,
        bool esAlVencimiento,
        long componenteCapitalId,
        long componenteInteresId,
        long estadoComponentePendienteId)
    {
        var componentes = new List<PrestamoComponente>();

        if (esAlVencimiento)
        {
            // Préstamo al vencimiento: 1 cuota de capital + 1 cuota de interés al final
            var fechaVencimiento = fechaDesembolso.AddDays(frecuenciaPagoDias * numeroCuotas);

            // Calcular interés total
            var tasaMensual = tasaInteresAnual / 12 / 100;
            var meses = (decimal)numeroCuotas * (frecuenciaPagoDias / 30m);
            var interesTotal = montoPrestamo * tasaMensual * meses;

            // Componente de Capital
            componentes.Add(new PrestamoComponente
            {
                PrestamoId = prestamoId,
                ComponentePrestamoId = componenteCapitalId,
                EstadoComponenteId = estadoComponentePendienteId,
                PrestamoComponenteMonto = montoPrestamo,
                PrestamoComponenteSaldo = montoPrestamo,
                PrestamoComponenteFechaVencimiento = fechaVencimiento,
                PrestamoComponenteNumeroCuota = 1
            });

            // Componente de Interés
            componentes.Add(new PrestamoComponente
            {
                PrestamoId = prestamoId,
                ComponentePrestamoId = componenteInteresId,
                EstadoComponenteId = estadoComponentePendienteId,
                PrestamoComponenteMonto = interesTotal,
                PrestamoComponenteSaldo = interesTotal,
                PrestamoComponenteFechaVencimiento = fechaVencimiento,
                PrestamoComponenteNumeroCuota = 1
            });
        }
        else
        {
            // Préstamo con cuotas periódicas
            var tasaMensual = tasaInteresAnual / 12 / 100;
            var factorDias = frecuenciaPagoDias / 30m;
            var tasaPeriodo = tasaMensual * factorDias;

            // Calcular cuota fija usando fórmula de amortización francesa
            var cuotaFija = CalcularCuotaFija(montoPrestamo, tasaPeriodo, numeroCuotas);

            var saldoCapital = montoPrestamo;
            var fechaPago = fechaDesembolso;

            for (int i = 1; i <= numeroCuotas; i++)
            {
                // Calcular fecha de pago
                fechaPago = fechaDesembolso.AddDays(frecuenciaPagoDias * i);

                // Calcular interés de la cuota
                var interesCuota = saldoCapital * tasaPeriodo;

                // Calcular capital de la cuota
                var capitalCuota = cuotaFija - interesCuota;

                // Ajustar última cuota si hay diferencias por redondeo
                if (i == numeroCuotas)
                {
                    capitalCuota = saldoCapital;
                    interesCuota = cuotaFija - capitalCuota;
                }

                // Componente de Capital
                componentes.Add(new PrestamoComponente
                {
                    PrestamoId = prestamoId,
                    ComponentePrestamoId = componenteCapitalId,
                    EstadoComponenteId = estadoComponentePendienteId,
                    PrestamoComponenteMonto = capitalCuota,
                    PrestamoComponenteSaldo = capitalCuota,
                    PrestamoComponenteFechaVencimiento = fechaPago,
                    PrestamoComponenteNumeroCuota = i
                });

                // Componente de Interés
                componentes.Add(new PrestamoComponente
                {
                    PrestamoId = prestamoId,
                    ComponentePrestamoId = componenteInteresId,
                    EstadoComponenteId = estadoComponentePendienteId,
                    PrestamoComponenteMonto = interesCuota,
                    PrestamoComponenteSaldo = interesCuota,
                    PrestamoComponenteFechaVencimiento = fechaPago,
                    PrestamoComponenteNumeroCuota = i
                });

                // Actualizar saldo de capital
                saldoCapital -= capitalCuota;
            }
        }

        return componentes;
    }

    /// <summary>
    /// Calcula la cuota fija usando la fórmula de amortización francesa
    /// Cuota = Monto * [i * (1 + i)^n] / [(1 + i)^n - 1]
    /// </summary>
    private decimal CalcularCuotaFija(decimal monto, decimal tasaPeriodo, int numeroCuotas)
    {
        if (tasaPeriodo == 0)
        {
            return monto / numeroCuotas;
        }

        var tasaDecimal = (double)tasaPeriodo;
        var factorPotencia = Math.Pow(1 + tasaDecimal, numeroCuotas);
        var cuota = (decimal)(((double)monto * tasaDecimal * factorPotencia) / (factorPotencia - 1));

        return Math.Round(cuota, 2);
    }

    /// <summary>
    /// Genera tabla de amortización para visualización
    /// </summary>
    public List<CuotaAmortizacion> GenerarTablaAmortizacion(
        decimal montoPrestamo,
        decimal tasaInteresAnual,
        int numeroCuotas,
        short frecuenciaPagoDias,
        DateTime fechaDesembolso,
        bool esAlVencimiento)
    {
        var tabla = new List<CuotaAmortizacion>();

        if (esAlVencimiento)
        {
            var tasaMensual = tasaInteresAnual / 12 / 100;
            var meses = (decimal)numeroCuotas * (frecuenciaPagoDias / 30m);
            var interesTotal = montoPrestamo * tasaMensual * meses;
            var fechaVencimiento = fechaDesembolso.AddDays(frecuenciaPagoDias * numeroCuotas);

            tabla.Add(new CuotaAmortizacion
            {
                NumeroCuota = 1,
                FechaPago = fechaVencimiento,
                Capital = montoPrestamo,
                Interes = interesTotal,
                Cuota = montoPrestamo + interesTotal,
                SaldoCapital = 0
            });
        }
        else
        {
            var tasaMensual = tasaInteresAnual / 12 / 100;
            var factorDias = frecuenciaPagoDias / 30m;
            var tasaPeriodo = tasaMensual * factorDias;
            var cuotaFija = CalcularCuotaFija(montoPrestamo, tasaPeriodo, numeroCuotas);

            var saldoCapital = montoPrestamo;
            var fechaPago = fechaDesembolso;

            for (int i = 1; i <= numeroCuotas; i++)
            {
                fechaPago = fechaDesembolso.AddDays(frecuenciaPagoDias * i);
                var interesCuota = saldoCapital * tasaPeriodo;
                var capitalCuota = cuotaFija - interesCuota;

                if (i == numeroCuotas)
                {
                    capitalCuota = saldoCapital;
                }

                tabla.Add(new CuotaAmortizacion
                {
                    NumeroCuota = i,
                    FechaPago = fechaPago,
                    Capital = capitalCuota,
                    Interes = interesCuota,
                    Cuota = capitalCuota + interesCuota,
                    SaldoCapital = saldoCapital - capitalCuota
                });

                saldoCapital -= capitalCuota;
            }
        }

        return tabla;
    }
}

/// <summary>
/// Clase auxiliar para representar una cuota en la tabla de amortización
/// </summary>
public class CuotaAmortizacion
{
    public int NumeroCuota { get; set; }
    public DateTime FechaPago { get; set; }
    public decimal Capital { get; set; }
    public decimal Interes { get; set; }
    public decimal Cuota { get; set; }
    public decimal SaldoCapital { get; set; }
}
