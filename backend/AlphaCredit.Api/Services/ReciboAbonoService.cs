using AlphaCredit.Api.Data;
using AlphaCredit.Api.DTOs;
using AlphaCredit.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlphaCredit.Api.Services;

/// <summary>
/// Servicio para generar recibos de abonos a préstamos en PDF
/// </summary>
public class ReciboAbonoService
{
    private readonly AlphaCreditDbContext _context;
    private readonly CurrencySettings _currencySettings;

    public ReciboAbonoService(
        AlphaCreditDbContext context,
        IOptions<CurrencySettings> currencySettings)
    {
        _context = context;
        _currencySettings = currencySettings.Value;
    }

    /// <summary>
    /// Genera un recibo de pago en PDF para un movimiento de préstamo
    /// </summary>
    public async Task<byte[]> GenerarReciboPdfAsync(long movimientoPrestamoId)
    {
        var datos = await ObtenerDatosReciboAsync(movimientoPrestamoId);
        var tipoRecibo = await ObtenerTipoReciboAsync();

        Document documento;

        if (tipoRecibo == "POS")
        {
            documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Formato para impresora POS (80mm = ~226 puntos)
                    page.Size(226, 10000); // ancho 80mm, largo dinámico
                    page.Margin(5);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Content().Element(c => CrearReciboPOS(c, datos));
                });
            });
        }
        else
        {
            // Formato A4 (actual)
            documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(c => CrearEncabezado(c, datos));
                    page.Content().Element(c => CrearContenido(c, datos));
                    page.Footer().Element(c => CrearPiePagina(c, datos));
                });
            });
        }

        return documento.GeneratePdf();
    }

    private async Task<string> ObtenerTipoReciboAsync()
    {
        var parametro = await _context.Set<ParametrosSistema>()
            .FirstOrDefaultAsync(p => p.ParametroSistemaLlave == "TIPO_RECIBO");

        return parametro?.ParametrosSistemaValor ?? "A4";
    }

    private void CrearEncabezado(IContainer container, DatosReciboDto datos)
    {
        container.Column(column =>
        {
            // Encabezado de la empresa
            column.Item().BorderBottom(2).PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(datos.EmpresaNombre)
                        .FontSize(16)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    if (!string.IsNullOrEmpty(datos.EmpresaDireccion))
                        col.Item().Text(datos.EmpresaDireccion).FontSize(9);

                    if (!string.IsNullOrEmpty(datos.EmpresaTelefono))
                        col.Item().Text($"Tel: {datos.EmpresaTelefono}").FontSize(9);
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text("RECIBO DE PAGO")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Red.Darken2);

                    col.Item().Text($"No. {datos.MovimientoId}")
                        .FontSize(12)
                        .Bold();

                    col.Item().Text(datos.FechaPago.ToString("dd/MM/yyyy"))
                        .FontSize(10);
                });
            });

            // Información del préstamo
            column.Item().PaddingTop(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("DATOS DEL PRÉSTAMO").FontSize(11).Bold().FontColor(Colors.Grey.Darken3);
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.ConstantItem(100).Text("Préstamo No:").Bold();
                        r.RelativeItem().Text(datos.PrestamoNumero);
                    });
                    col.Item().Row(r =>
                    {
                        r.ConstantItem(100).Text("Sucursal:").Bold();
                        r.RelativeItem().Text(datos.SucursalNombre);
                    });
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("DATOS DEL CLIENTE").FontSize(11).Bold().FontColor(Colors.Grey.Darken3);
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.ConstantItem(80).Text("Nombre:").Bold();
                        r.RelativeItem().Text(datos.ClienteNombre);
                    });
                    col.Item().Row(r =>
                    {
                        r.ConstantItem(80).Text("Cédula:").Bold();
                        r.RelativeItem().Text(datos.ClienteCedula);
                    });
                    if (!string.IsNullOrEmpty(datos.ClienteTelefono))
                    {
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(80).Text("Teléfono:").Bold();
                            r.RelativeItem().Text(datos.ClienteTelefono);
                        });
                    }
                });
            });
        });
    }

    private void CrearContenido(IContainer container, DatosReciboDto datos)
    {
        container.PaddingTop(20).Column(column =>
        {
            // Tipo de abono
            column.Item().Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
            {
                row.RelativeItem().Text($"TIPO DE ABONO: {datos.TipoMovimiento}").FontSize(11).Bold();
                row.ConstantItem(150).AlignRight().Text($"Forma de Pago: {datos.FormaPago}").FontSize(10);
            });

            // Detalle del pago
            column.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(200);
                    columns.RelativeColumn();
                    columns.ConstantColumn(100);
                });

                // Encabezado
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text("Concepto").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text("Detalle").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight()
                        .Text("Monto").FontColor(Colors.White).Bold();
                });

                // Capital
                if (datos.MontoCapital > 0)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text("CAPITAL");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text($"{datos.ComponentesAfectados.Count(c => c.ComponenteNombre.Contains("Capital"))} componente(s)");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                        .Text(FormatearMoneda(datos.MontoCapital));
                }

                // Interés
                if (datos.MontoInteres > 0)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text("INTERÉS");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text($"{datos.ComponentesAfectados.Count(c => c.ComponenteNombre.Contains("Interés") || c.ComponenteNombre.Contains("Interes"))} componente(s)");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                        .Text(FormatearMoneda(datos.MontoInteres));
                }

                // Mora
                if (datos.MontoMora > 0)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text("MORA");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text($"{datos.ComponentesAfectados.Count(c => c.ComponenteNombre.Contains("Mora"))} componente(s)");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                        .Text(FormatearMoneda(datos.MontoMora));
                }

                // Otros
                if (datos.MontoOtros > 0)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text("OTROS");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text("Otros cargos");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight()
                        .Text(FormatearMoneda(datos.MontoOtros));
                }

                // Total
                table.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                    .Text("TOTAL PAGADO").Bold().FontSize(11);
                table.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                    .Text("");
                table.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight()
                    .Text(FormatearMoneda(datos.MontoTotal)).Bold().FontSize(12);
            });

            // Monto en letras
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text($"Son: {ConvertirNumeroALetras(datos.MontoTotal)} {_currencySettings.Name}")
                    .Italic()
                    .FontSize(10);
            });

            // Detalle de componentes afectados
            if (datos.ComponentesAfectados.Any())
            {
                column.Item().PaddingTop(15).Column(col =>
                {
                    col.Item().Text("DETALLE DE COMPONENTES AFECTADOS").FontSize(10).Bold();

                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(150);
                            columns.RelativeColumn();
                            columns.ConstantColumn(90);
                            columns.ConstantColumn(90);
                        });

                        // Encabezado
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Cuota").FontSize(8).Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Componente").FontSize(8).Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Estado").FontSize(8).Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Saldo Antes").FontSize(8).Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(3).AlignRight().Text("Monto Aplicado").FontSize(8).Bold();
                        });

                        foreach (var componente in datos.ComponentesAfectados.OrderBy(c => c.NumeroCuota))
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3)
                                .Text(componente.NumeroCuota?.ToString() ?? "-").FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3)
                                .Text(componente.ComponenteNombre).FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3)
                                .Text(componente.EstadoNuevo).FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3).AlignRight()
                                .Text(FormatearMoneda(componente.MontoAntes)).FontSize(8);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(3).AlignRight()
                                .Text(FormatearMoneda(componente.MontoAplicado)).FontSize(8);
                        }
                    });
                });
            }

            // Saldos después del pago
            column.Item().PaddingTop(15).Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
            {
                col.Item().Text("SALDOS DESPUÉS DEL PAGO").FontSize(10).Bold();
                col.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Saldo Capital: {FormatearMoneda(datos.SaldoCapitalDespues)}");
                    row.RelativeItem().Text($"Saldo Interés: {FormatearMoneda(datos.SaldoInteresDespues)}");
                    row.RelativeItem().Text($"Saldo Mora: {FormatearMoneda(datos.SaldoMoraDespues)}");
                });
                col.Item().PaddingTop(3).Text($"Saldo Total Pendiente: {FormatearMoneda(datos.SaldoTotalDespues)}").Bold().FontSize(11);
            });

            // Observaciones
            if (!string.IsNullOrEmpty(datos.Observaciones))
            {
                column.Item().PaddingTop(10).Column(col =>
                {
                    col.Item().Text("OBSERVACIONES:").FontSize(9).Bold();
                    col.Item().Text(datos.Observaciones).FontSize(9);
                });
            }
        });
    }

    private void CrearPiePagina(IContainer container, DatosReciboDto datos)
    {
        container.PaddingTop(40).Column(column =>
        {
            // Firma del cliente
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().BorderTop(1).BorderColor(Colors.Grey.Darken1).PaddingTop(5).AlignCenter()
                        .Text("Firma del Cliente").FontSize(9);
                    col.Item().AlignCenter().Text(datos.ClienteNombre).FontSize(8).Italic();
                    col.Item().AlignCenter().Text($"Cédula: {datos.ClienteCedula}").FontSize(8);
                });

                row.ConstantItem(50);

                row.RelativeItem().Column(col =>
                {
                    col.Item().BorderTop(1).BorderColor(Colors.Grey.Darken1).PaddingTop(5).AlignCenter()
                        .Text("Cajero(a)").FontSize(9);
                    col.Item().AlignCenter().Text(datos.UsuarioCrea ?? "SISTEMA").FontSize(8).Italic();
                });
            });

            // Información adicional
            column.Item().PaddingTop(20).AlignCenter().Column(col =>
            {
                col.Item().Text($"Fecha y hora de emisión: {datos.FechaCreacion:dd/MM/yyyy HH:mm:ss}")
                    .FontSize(7)
                    .FontColor(Colors.Grey.Darken1);

                if (!string.IsNullOrEmpty(datos.DocumentoReferencia))
                {
                    col.Item().Text($"Documento de referencia: {datos.DocumentoReferencia}")
                        .FontSize(7)
                        .FontColor(Colors.Grey.Darken1);
                }

                col.Item().PaddingTop(5).Text("Este documento es un comprobante válido de pago")
                    .FontSize(7)
                    .Italic()
                    .FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private async Task<DatosReciboDto> ObtenerDatosReciboAsync(long movimientoPrestamoId)
    {
        var movimiento = await _context.MovimientosPrestamo
            .Include(m => m.Prestamo)
                .ThenInclude(p => p.Persona)
                    .ThenInclude(p => p.PersonaTelefonos)
            .Include(m => m.Prestamo)
                .ThenInclude(p => p.Sucursal)
                    .ThenInclude(s => s.Empresa)
            .Include(m => m.FormaPago)
            .FirstOrDefaultAsync(m => m.MovimientoPrestamoId == movimientoPrestamoId);

        if (movimiento == null)
            throw new InvalidOperationException($"Movimiento de préstamo {movimientoPrestamoId} no encontrado");

        var componentesAfectados = await _context.Set<Models.PagoDetalle>()
            .Include(pd => pd.ComponentePrestamo)
            .Include(pd => pd.PrestamoComponente)
            .Where(pd => pd.MovimientoPrestamoId == movimientoPrestamoId)
            .Select(pd => new ComponenteAfectadoDto
            {
                PrestamoComponenteId = pd.PrestamoComponenteId,
                ComponenteNombre = pd.ComponentePrestamo.ComponentePrestamoNombre,
                NumeroCuota = pd.PagoDetalleCuotaNumero,
                MontoAntes = pd.PagoDetalleMontoAntes,
                MontoAplicado = pd.PagoDetalleMontoAplicado,
                SaldoNuevo = pd.PrestamoComponente != null ? pd.PrestamoComponente.PrestamoComponenteSaldo : 0,
                EstadoNuevo = pd.PrestamoComponente != null && pd.PrestamoComponente.PrestamoComponenteSaldo == 0 ? "Pagado" : "Pendiente"
            })
            .ToListAsync();

        return new DatosReciboDto
        {
            MovimientoId = movimiento.MovimientoPrestamoId,
            FechaPago = movimiento.MovimientoPrestamoFecha,
            FechaCreacion = movimiento.MovimientoPrestamoFechaCreacion,
            TipoMovimiento = movimiento.MovimientoPrestamoTipo,

            // Empresa
            EmpresaNombre = movimiento.Prestamo.Sucursal.Empresa.EmpresaNombre,
            EmpresaDireccion = movimiento.Prestamo.Sucursal.Empresa.EmpresaDireccion,
            EmpresaTelefono = movimiento.Prestamo.Sucursal.Empresa.EmpresaTelefono,

            // Sucursal
            SucursalNombre = movimiento.Prestamo.Sucursal.SucursalNombre,

            // Préstamo
            PrestamoNumero = movimiento.Prestamo.PrestamoNumero,

            // Cliente
            ClienteNombre = $"{movimiento.Prestamo.Persona.PersonaPrimerNombre} {movimiento.Prestamo.Persona.PersonaSegundoNombre} {movimiento.Prestamo.Persona.PersonaPrimerApellido} {movimiento.Prestamo.Persona.PersonaSegundoApellido}".Trim(),
            ClienteCedula = movimiento.Prestamo.Persona.PersonaIdentificacion,
            ClienteTelefono = movimiento.Prestamo.Persona.PersonaTelefonos.FirstOrDefault()?.PersonaTelefoNumero ?? "N/A",

            // Montos
            MontoCapital = movimiento.MovimientoPrestamoMontoCapital,
            MontoInteres = movimiento.MovimientoPrestamoMontoInteres,
            MontoMora = movimiento.MovimientoPrestamoMontoMora,
            MontoOtros = movimiento.MovimientoPrestamoMontoOtros,
            MontoTotal = movimiento.MovimientoPrestamoMontoTotal,

            // Forma de pago
            FormaPago = movimiento.FormaPago?.FormaPagoNombre ?? "N/A",
            DocumentoReferencia = movimiento.MovimientoPrestamoObservaciones,

            // Saldos después del pago
            SaldoCapitalDespues = movimiento.Prestamo.PrestamoSaldoCapital,
            SaldoInteresDespues = movimiento.Prestamo.PrestamoSaldoInteres,
            SaldoMoraDespues = movimiento.Prestamo.PrestamoSaldoMora,
            SaldoTotalDespues = movimiento.Prestamo.PrestamoSaldoCapital + movimiento.Prestamo.PrestamoSaldoInteres + movimiento.Prestamo.PrestamoSaldoMora,

            // Componentes afectados
            ComponentesAfectados = componentesAfectados,

            // Usuario
            UsuarioCrea = movimiento.MovimientoPrestamoUserCrea,
            Observaciones = movimiento.MovimientoPrestamoObservaciones
        };
    }

    private string FormatearMoneda(decimal monto)
    {
        return $"{_currencySettings.Symbol} {monto:N2}";
    }

    private void CrearReciboPOS(IContainer container, DatosReciboDto datos)
    {
        container.PaddingVertical(5).Column(column =>
        {
            // Encabezado empresa - centrado
            column.Item().AlignCenter().Column(col =>
            {
                col.Item().Text(datos.EmpresaNombre).Bold().FontSize(10);
                if (!string.IsNullOrEmpty(datos.SucursalNombre))
                    col.Item().Text(datos.SucursalNombre).FontSize(7);
                if (!string.IsNullOrEmpty(datos.EmpresaDireccion))
                    col.Item().Text(datos.EmpresaDireccion).FontSize(6);
                if (!string.IsNullOrEmpty(datos.EmpresaTelefono))
                    col.Item().Text($"Tel: {datos.EmpresaTelefono}").FontSize(6);
            });

            // Línea separadora
            column.Item().PaddingVertical(3).LineHorizontal(0.5f);

            // Título del recibo
            column.Item().AlignCenter().Text("RECIBO DE PAGO").Bold().FontSize(9);
            column.Item().AlignCenter().Text($"No. {datos.MovimientoId}").FontSize(7);

            column.Item().PaddingVertical(2).LineHorizontal(0.5f);

            // Información del pago
            column.Item().Column(col =>
            {
                col.Item().Text($"Fecha: {datos.FechaPago:dd/MM/yyyy HH:mm}").FontSize(7);
                col.Item().Text($"Préstamo: {datos.PrestamoNumero}").FontSize(7);
                col.Item().Text($"Cliente: {datos.ClienteNombre}").FontSize(7);
                if (!string.IsNullOrEmpty(datos.ClienteCedula))
                    col.Item().Text($"Cédula: {datos.ClienteCedula}").FontSize(7);
            });

            column.Item().PaddingVertical(2).LineHorizontal(0.5f);

            // Detalle del pago - tabla compacta
            column.Item().Text("DETALLE DEL PAGO").Bold().FontSize(8);

            column.Item().PaddingTop(2).Column(col =>
            {
                if (datos.MontoCapital > 0)
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Capital:").FontSize(7);
                        row.RelativeItem().AlignRight().Text(FormatearMoneda(datos.MontoCapital)).FontSize(7);
                    });
                }

                if (datos.MontoInteres > 0)
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Interés:").FontSize(7);
                        row.RelativeItem().AlignRight().Text(FormatearMoneda(datos.MontoInteres)).FontSize(7);
                    });
                }

                if (datos.MontoMora > 0)
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Mora:").FontSize(7);
                        row.RelativeItem().AlignRight().Text(FormatearMoneda(datos.MontoMora)).FontSize(7);
                    });
                }

                if (datos.MontoOtros > 0)
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Otros:").FontSize(7);
                        row.RelativeItem().AlignRight().Text(FormatearMoneda(datos.MontoOtros)).FontSize(7);
                    });
                }
            });

            column.Item().PaddingVertical(2).LineHorizontal(0.5f);

            // Total
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("TOTAL PAGADO:").Bold().FontSize(8);
                row.RelativeItem().AlignRight().Text(FormatearMoneda(datos.MontoTotal)).Bold().FontSize(9);
            });

            // Monto en letras
            column.Item().PaddingTop(2).Text($"({ConvertirNumeroALetras(datos.MontoTotal)} {_currencySettings.Name})")
                .FontSize(6).Italic();

            column.Item().PaddingVertical(2).LineHorizontal(0.5f);

            // Forma de pago
            column.Item().Text($"Forma de pago: {datos.FormaPago}").FontSize(7);
            if (!string.IsNullOrEmpty(datos.DocumentoReferencia))
                column.Item().Text($"Ref: {datos.DocumentoReferencia}").FontSize(6);

            column.Item().PaddingVertical(2).LineHorizontal(0.5f);

            // Saldos después del pago
            column.Item().Text("SALDOS PENDIENTES").Bold().FontSize(8);
            column.Item().PaddingTop(2).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Capital:").FontSize(7);
                    row.RelativeItem().AlignRight().Text(FormatearMoneda(datos.SaldoCapitalDespues)).FontSize(7);
                });

                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Interés:").FontSize(7);
                    row.RelativeItem().AlignRight().Text(FormatearMoneda(datos.SaldoInteresDespues)).FontSize(7);
                });

                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Mora:").FontSize(7);
                    row.RelativeItem().AlignRight().Text(FormatearMoneda(datos.SaldoMoraDespues)).FontSize(7);
                });

                col.Item().PaddingTop(1).Row(row =>
                {
                    row.RelativeItem().Text("TOTAL:").Bold().FontSize(8);
                    row.RelativeItem().AlignRight().Text(FormatearMoneda(datos.SaldoTotalDespues)).Bold().FontSize(8);
                });
            });

            // Observaciones si existen
            if (!string.IsNullOrEmpty(datos.Observaciones))
            {
                column.Item().PaddingVertical(2).LineHorizontal(0.5f);
                column.Item().Text("Observaciones:").FontSize(7).Bold();
                column.Item().Text(datos.Observaciones).FontSize(6);
            }

            column.Item().PaddingVertical(3).LineHorizontal(0.5f);

            // Firma del cliente
            column.Item().PaddingTop(15).Column(col =>
            {
                col.Item().LineHorizontal(0.5f);
                col.Item().AlignCenter().Text("Firma del Cliente").FontSize(6);
            });

            // Pie de página
            column.Item().PaddingTop(5).AlignCenter().Column(col =>
            {
                col.Item().Text("Gracias por su pago").FontSize(7).Bold();
                col.Item().Text($"Generado: {datos.FechaCreacion:dd/MM/yyyy HH:mm}").FontSize(5);
            });

            // Espacio adicional antes del corte (para impresoras térmicas POS)
            // Esto asegura que todo el contenido se imprima antes del corte
            column.Item().PaddingTop(10).Text(" ").FontSize(1);
        });
    }

    private string ConvertirNumeroALetras(decimal numero)
    {
        // Implementación básica - puedes mejorarla
        var parteEntera = (long)numero;
        var parteDecimal = (int)((numero - parteEntera) * 100);

        if (parteEntera == 0)
            return $"CERO CON {parteDecimal:00}/100";

        // Aquí puedes agregar lógica más compleja para convertir números a letras
        return $"{parteEntera:N0} CON {parteDecimal:00}/100";
    }
}

public class DatosReciboDto
{
    public long MovimientoId { get; set; }
    public DateTime FechaPago { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;

    public string EmpresaNombre { get; set; } = string.Empty;
    public string? EmpresaDireccion { get; set; }
    public string? EmpresaTelefono { get; set; }

    public string SucursalNombre { get; set; } = string.Empty;

    public string PrestamoNumero { get; set; } = string.Empty;

    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteCedula { get; set; } = string.Empty;
    public string? ClienteTelefono { get; set; }

    public decimal MontoCapital { get; set; }
    public decimal MontoInteres { get; set; }
    public decimal MontoMora { get; set; }
    public decimal MontoOtros { get; set; }
    public decimal MontoTotal { get; set; }

    public string FormaPago { get; set; } = string.Empty;
    public string? DocumentoReferencia { get; set; }

    public decimal SaldoCapitalDespues { get; set; }
    public decimal SaldoInteresDespues { get; set; }
    public decimal SaldoMoraDespues { get; set; }
    public decimal SaldoTotalDespues { get; set; }

    public List<ComponenteAfectadoDto> ComponentesAfectados { get; set; } = new();

    public string? UsuarioCrea { get; set; }
    public string? Observaciones { get; set; }
}
