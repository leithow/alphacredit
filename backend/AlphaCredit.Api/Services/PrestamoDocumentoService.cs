using System.Text;
using AlphaCredit.Api.Data;
using AlphaCredit.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AlphaCredit.Api.Services;

public class PrestamoDocumentoService
{
    private readonly AlphaCreditDbContext _context;
    private readonly PrestamoAmortizacionService _amortizacionService;

    public PrestamoDocumentoService(AlphaCreditDbContext context)
    {
        _context = context;
        _amortizacionService = new PrestamoAmortizacionService();
    }

    /// <summary>
    /// Genera el contrato de préstamo en HTML/texto basado en normativa hondureña
    /// </summary>
    public async Task<string> GenerarContratoPrestamo(long prestamoId)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.Sucursal)
            .Include(p => p.Moneda)
            .Include(p => p.FrecuenciaPago)
            .Include(p => p.DestinoCredito)
            .Include(p => p.PrestamoGarantias)
                .ThenInclude(pg => pg.Garantia)
                    .ThenInclude(g => g.TipoGarantia)
            .FirstOrDefaultAsync(p => p.PrestamoId == prestamoId);

        if (prestamo == null)
            throw new Exception($"Préstamo {prestamoId} no encontrado");

        var html = new StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang='es'>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset='UTF-8'>");
        html.AppendLine("    <title>Contrato de Préstamo</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }");
        html.AppendLine("        .header { text-align: center; margin-bottom: 30px; }");
        html.AppendLine("        .title { font-size: 18px; font-weight: bold; margin-bottom: 10px; }");
        html.AppendLine("        .section { margin-bottom: 20px; }");
        html.AppendLine("        .clause { margin-bottom: 15px; text-align: justify; }");
        html.AppendLine("        .clause-title { font-weight: bold; }");
        html.AppendLine("        .signature-section { margin-top: 60px; }");
        html.AppendLine("        .signature-line { border-top: 1px solid #000; width: 300px; margin: 40px auto 5px; }");
        html.AppendLine("        .signature-label { text-align: center; font-size: 12px; }");
        html.AppendLine("        .info-table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
        html.AppendLine("        .info-table td { padding: 5px; border: 1px solid #ddd; }");
        html.AppendLine("        .info-table td:first-child { font-weight: bold; background: #f5f5f5; width: 200px; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Encabezado
        html.AppendLine("    <div class='header'>");
        html.AppendLine("        <div class='title'>CONTRATO DE PRÉSTAMO</div>");
        html.AppendLine($"        <div>Contrato No. {prestamo.PrestamoNumero}</div>");
        html.AppendLine($"        <div>Fecha: {DateTime.Now:dd/MM/yyyy}</div>");
        html.AppendLine("    </div>");

        // Comparecientes
        html.AppendLine("    <div class='section'>");
        html.AppendLine("        <div class='clause'>");
        html.AppendLine($"            Nosotros, <strong>ALPHACREDIT</strong>, por medio de su representante legal, en adelante denominada <strong>EL ACREEDOR</strong>, ");
        html.AppendLine($"            y <strong>{prestamo.Persona.PersonaNombreCompleto.ToUpper()}</strong>, ");
        html.AppendLine($"            con número de identificación <strong>{prestamo.Persona.PersonaIdentificacion}</strong>, ");
        html.AppendLine($"            en adelante denominado <strong>EL DEUDOR</strong>, convenimos celebrar el presente contrato de préstamo de dinero, ");
        html.AppendLine("            el cual se regirá por las siguientes cláusulas:");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");

        // Datos del préstamo
        html.AppendLine("    <table class='info-table'>");
        html.AppendLine($"        <tr><td>Monto del Préstamo:</td><td>{prestamo.PrestamoMonto:N2} {prestamo.Moneda.MonedaCodigo}</td></tr>");
        html.AppendLine($"        <tr><td>Tasa de Interés Anual:</td><td>{prestamo.PrestamoTasaInteres}%</td></tr>");
        html.AppendLine($"        <tr><td>Plazo:</td><td>{prestamo.PrestamoPlazo} cuotas ({prestamo.FrecuenciaPago.FrecuenciaPagoNombre})</td></tr>");
        html.AppendLine($"        <tr><td>Fecha de Desembolso:</td><td>{prestamo.PrestamoFechaDesembolso:dd/MM/yyyy}</td></tr>");
        html.AppendLine($"        <tr><td>Fecha de Vencimiento:</td><td>{prestamo.PrestamoFechaVencimiento:dd/MM/yyyy}</td></tr>");
        if (prestamo.DestinoCredito != null)
        {
            html.AppendLine($"        <tr><td>Destino del Crédito:</td><td>{prestamo.DestinoCredito.DestinoCreditoNombre}</td></tr>");
        }
        html.AppendLine("    </table>");

        // Cláusulas
        html.AppendLine("    <div class='section'>");

        html.AppendLine("        <div class='clause'>");
        html.AppendLine("            <div class='clause-title'>PRIMERA: DEL OBJETO.</div>");
        html.AppendLine($"            Por medio del presente contrato, <strong>EL ACREEDOR</strong> entrega a <strong>EL DEUDOR</strong> la suma de ");
        html.AppendLine($"            <strong>{prestamo.PrestamoMonto:N2} {prestamo.Moneda.MonedaNombre}</strong> ({NumeroALetras(prestamo.PrestamoMonto)} {prestamo.Moneda.MonedaNombre}), ");
        html.AppendLine($"            y <strong>EL DEUDOR</strong> se obliga a devolver dicha suma más los intereses pactados.");
        html.AppendLine("        </div>");

        html.AppendLine("        <div class='clause'>");
        html.AppendLine("            <div class='clause-title'>SEGUNDA: DE LOS INTERESES.</div>");
        html.AppendLine($"            <strong>EL DEUDOR</strong> se compromete a pagar una tasa de interés de <strong>{prestamo.PrestamoTasaInteres}% anual</strong> ");
        html.AppendLine("            sobre el saldo de capital adeudado. Los intereses se calcularán sobre el saldo insoluto y se pagarán según el plan de amortización acordado.");
        html.AppendLine("        </div>");

        html.AppendLine("        <div class='clause'>");
        html.AppendLine("            <div class='clause-title'>TERCERA: DEL PLAZO Y FORMA DE PAGO.</div>");
        html.AppendLine($"            <strong>EL DEUDOR</strong> se obliga a cancelar el préstamo en <strong>{prestamo.PrestamoPlazo} cuotas {prestamo.FrecuenciaPago.FrecuenciaPagoNombre.ToLower()}</strong>, ");
        html.AppendLine($"            según el plan de amortización anexo a este contrato. El plazo vence el {prestamo.PrestamoFechaVencimiento:dd/MM/yyyy}.");
        html.AppendLine("        </div>");

        html.AppendLine("        <div class='clause'>");
        html.AppendLine("            <div class='clause-title'>CUARTA: DE LA MORA.</div>");
        html.AppendLine("            En caso de mora en el pago de una o más cuotas, <strong>EL DEUDOR</strong> se obliga a pagar intereses moratorios ");
        html.AppendLine("            sobre el monto vencido, conforme a lo establecido en la legislación vigente de Honduras. ");
        html.AppendLine("            La mora en el pago de dos (2) o más cuotas consecutivas dará derecho a <strong>EL ACREEDOR</strong> ");
        html.AppendLine("            para declarar vencido el plazo del contrato y exigir el pago inmediato del saldo total adeudado.");
        html.AppendLine("        </div>");

        // Garantías si existen
        if (prestamo.PrestamoGarantias.Any())
        {
            html.AppendLine("        <div class='clause'>");
            html.AppendLine("            <div class='clause-title'>QUINTA: DE LAS GARANTÍAS.</div>");
            html.AppendLine("            Para garantizar el cumplimiento de las obligaciones contraídas, <strong>EL DEUDOR</strong> constituye las siguientes garantías:");
            html.AppendLine("            <ul>");
            foreach (var pg in prestamo.PrestamoGarantias.Where(pg => pg.PrestamoGarantiaEstaActiva))
            {
                html.AppendLine($"                <li><strong>{pg.Garantia.TipoGarantia.TipoGarantiaNombre}:</strong> {pg.Garantia.GarantiaDescripcion} ");
                html.AppendLine($"                    por un monto comprometido de {pg.PrestamoGarantiaMontoComprometido:N2} {prestamo.Moneda.MonedaCodigo}</li>");
            }
            html.AppendLine("            </ul>");
            html.AppendLine("        </div>");
        }

        html.AppendLine("        <div class='clause'>");
        html.AppendLine($"            <div class='clause-title'>{(prestamo.PrestamoGarantias.Any() ? "SEXTA" : "QUINTA")}: DEL DOMICILIO.</div>");
        html.AppendLine($"            <strong>EL DEUDOR</strong> señala como domicilio: {prestamo.Persona.PersonaDireccion ?? "No especificado"}");
        html.AppendLine("        </div>");

        html.AppendLine("        <div class='clause'>");
        html.AppendLine($"            <div class='clause-title'>{(prestamo.PrestamoGarantias.Any() ? "SÉPTIMA" : "SEXTA")}: DE LA LEGISLACIÓN APLICABLE.</div>");
        html.AppendLine("            Este contrato se rige por las leyes de la República de Honduras, particularmente el Código de Comercio y el Código Civil.");
        html.AppendLine("        </div>");

        html.AppendLine("        <div class='clause'>");
        html.AppendLine($"            <div class='clause-title'>{(prestamo.PrestamoGarantias.Any() ? "OCTAVA" : "SÉPTIMA")}: DE LA ACEPTACIÓN.</div>");
        html.AppendLine("            Las partes manifiestan su plena conformidad con los términos del presente contrato y proceden a firmarlo ");
        html.AppendLine($"            en {prestamo.Sucursal.SucursalNombre}, a los {DateTime.Now.Day} días del mes de {DateTime.Now:MMMM} del año {DateTime.Now.Year}.");
        html.AppendLine("        </div>");

        html.AppendLine("    </div>");

        // Firmas
        html.AppendLine("    <div class='signature-section'>");
        html.AppendLine("        <div style='display: flex; justify-content: space-around; margin-top: 80px;'>");
        html.AppendLine("            <div>");
        html.AppendLine("                <div class='signature-line'></div>");
        html.AppendLine("                <div class='signature-label'>EL ACREEDOR</div>");
        html.AppendLine("                <div class='signature-label'>ALPHACREDIT</div>");
        html.AppendLine("            </div>");
        html.AppendLine("            <div>");
        html.AppendLine("                <div class='signature-line'></div>");
        html.AppendLine("                <div class='signature-label'>EL DEUDOR</div>");
        html.AppendLine($"                <div class='signature-label'>{prestamo.Persona.PersonaNombreCompleto}</div>");
        html.AppendLine($"                <div class='signature-label'>ID: {prestamo.Persona.PersonaIdentificacion}</div>");
        html.AppendLine("            </div>");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    /// <summary>
    /// Genera el pagaré del préstamo basado en normativa hondureña
    /// </summary>
    public async Task<string> GenerarPagare(long prestamoId)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.Sucursal)
            .Include(p => p.Moneda)
            .Include(p => p.FrecuenciaPago)
            .FirstOrDefaultAsync(p => p.PrestamoId == prestamoId);

        if (prestamo == null)
            throw new Exception($"Préstamo {prestamoId} no encontrado");

        // Calcular monto total a pagar (capital + intereses)
        var tablaAmortizacion = _amortizacionService.GenerarTablaAmortizacion(
            prestamo.PrestamoMonto,
            prestamo.PrestamoTasaInteres,
            prestamo.PrestamoPlazo,
            prestamo.FrecuenciaPago.FrecuenciaPagoDias,
            prestamo.PrestamoFechaDesembolso ?? DateTime.UtcNow,
            false
        );

        var totalAPagar = tablaAmortizacion.Sum(c => c.Cuota);

        var html = new StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang='es'>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset='UTF-8'>");
        html.AppendLine("    <title>Pagaré</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: 'Times New Roman', serif; margin: 60px; line-height: 2; }");
        html.AppendLine("        .header { text-align: center; margin-bottom: 40px; }");
        html.AppendLine("        .title { font-size: 24px; font-weight: bold; text-decoration: underline; }");
        html.AppendLine("        .content { text-align: justify; font-size: 14px; }");
        html.AppendLine("        .amount { font-weight: bold; text-decoration: underline; }");
        html.AppendLine("        .signature-section { margin-top: 100px; }");
        html.AppendLine("        .signature-line { border-top: 2px solid #000; width: 400px; margin: 80px auto 10px; }");
        html.AppendLine("        .signature-label { text-align: center; font-size: 12px; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine("    <div class='header'>");
        html.AppendLine("        <div class='title'>PAGARÉ</div>");
        html.AppendLine($"        <div>No. {prestamo.PrestamoNumero}</div>");
        html.AppendLine($"        <div>Por: {totalAPagar:N2} {prestamo.Moneda.MonedaCodigo}</div>");
        html.AppendLine("    </div>");

        html.AppendLine("    <div class='content'>");
        html.AppendLine($"        <p>DEBO Y PAGARÉ incondicionalmente por este PAGARÉ a la orden de <strong>ALPHACREDIT</strong>, ");
        html.AppendLine($"        o a quien represente sus derechos, en <strong>{prestamo.Sucursal.SucursalNombre}, Honduras</strong>, ");
        html.AppendLine($"        el día <strong>{prestamo.PrestamoFechaVencimiento:dd/MM/yyyy}</strong>, ");
        html.AppendLine($"        la suma de <span class='amount'>{totalAPagar:N2} {prestamo.Moneda.MonedaNombre}</span> ");
        html.AppendLine($"        (<span class='amount'>{NumeroALetras(totalAPagar)} {prestamo.Moneda.MonedaNombre}</span>), ");
        html.AppendLine($"        valor recibido a mi entera satisfacción.</p>");

        html.AppendLine($"        <p>Esta obligación devengará intereses a razón del <strong>{prestamo.PrestamoTasaInteres}% anual</strong> ");
        html.AppendLine($"        sobre el saldo de capital adeudado, pagaderos en <strong>{prestamo.PrestamoPlazo} cuotas {prestamo.FrecuenciaPago.FrecuenciaPagoNombre.ToLower()}</strong> ");
        html.AppendLine("         según el plan de amortización acordado.</p>");

        html.AppendLine("        <p>En caso de mora en el pago de alguna cuota, me comprometo a pagar los intereses moratorios correspondientes ");
        html.AppendLine("        según lo establecido por la legislación hondureña vigente. El incumplimiento en el pago de dos (2) o más cuotas ");
        html.AppendLine("        dará derecho al tenedor de este pagaré para declarar vencido el plazo y exigir el pago inmediato del saldo total adeudado.</p>");

        html.AppendLine("        <p>Para el cumplimiento de esta obligación señalo como domicilio especial el ubicado en: ");
        html.AppendLine($"        <strong>{prestamo.Persona.PersonaDireccion ?? "No especificado"}</strong>.</p>");

        html.AppendLine("        <p>Renuncio expresamente al beneficio de excusión y a cualquier otro beneficio que pudiera favorecerme ");
        html.AppendLine("        conforme al Código Civil y de Comercio de Honduras.</p>");

        html.AppendLine($"        <p>Extendido en <strong>{prestamo.Sucursal.SucursalNombre}, Honduras</strong>, ");
        html.AppendLine($"        a los <strong>{DateTime.Now.Day}</strong> días del mes de <strong>{DateTime.Now:MMMM}</strong> ");
        html.AppendLine($"        del año <strong>{DateTime.Now.Year}</strong>.</p>");

        html.AppendLine("    </div>");

        html.AppendLine("    <div class='signature-section'>");
        html.AppendLine("        <div class='signature-line'></div>");
        html.AppendLine("        <div class='signature-label'><strong>FIRMA DEL DEUDOR</strong></div>");
        html.AppendLine($"        <div class='signature-label'>{prestamo.Persona.PersonaNombreCompleto}</div>");
        html.AppendLine($"        <div class='signature-label'>Identidad: {prestamo.Persona.PersonaIdentificacion}</div>");
        html.AppendLine("    </div>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    /// <summary>
    /// Genera el plan de pagos del préstamo
    /// </summary>
    public async Task<string> GenerarPlanPagos(long prestamoId)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.Sucursal)
            .Include(p => p.Moneda)
            .Include(p => p.FrecuenciaPago)
            .Include(p => p.PrestamoComponentes)
            .FirstOrDefaultAsync(p => p.PrestamoId == prestamoId);

        if (prestamo == null)
            throw new Exception($"Préstamo {prestamoId} no encontrado");

        // Determinar si es al vencimiento
        var numeroComponentes = prestamo.PrestamoComponentes.Count;
        var esAlVencimiento = numeroComponentes == 2;

        var tablaAmortizacion = _amortizacionService.GenerarTablaAmortizacion(
            prestamo.PrestamoMonto,
            prestamo.PrestamoTasaInteres,
            prestamo.PrestamoPlazo,
            prestamo.FrecuenciaPago.FrecuenciaPagoDias,
            prestamo.PrestamoFechaDesembolso ?? DateTime.UtcNow,
            esAlVencimiento
        );

        var html = new StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang='es'>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset='UTF-8'>");
        html.AppendLine("    <title>Plan de Pagos</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 40px; }");
        html.AppendLine("        .header { text-align: center; margin-bottom: 30px; }");
        html.AppendLine("        .title { font-size: 20px; font-weight: bold; }");
        html.AppendLine("        .info { margin: 20px 0; }");
        html.AppendLine("        .info-row { display: flex; margin: 5px 0; }");
        html.AppendLine("        .info-label { font-weight: bold; width: 200px; }");
        html.AppendLine("        table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
        html.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: center; }");
        html.AppendLine("        th { background-color: #4CAF50; color: white; font-weight: bold; }");
        html.AppendLine("        tr:nth-child(even) { background-color: #f2f2f2; }");
        html.AppendLine("        .amount { text-align: right; }");
        html.AppendLine("        .total-row { font-weight: bold; background-color: #e3f2fd !important; }");
        html.AppendLine("        .summary { margin-top: 30px; padding: 20px; background: #f5f5f5; border-radius: 5px; }");
        html.AppendLine("        .summary-item { display: flex; justify-content: space-between; margin: 10px 0; font-size: 16px; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine("    <div class='header'>");
        html.AppendLine("        <div class='title'>PLAN DE PAGOS</div>");
        html.AppendLine($"        <div>Préstamo No. {prestamo.PrestamoNumero}</div>");
        html.AppendLine($"        <div>Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}</div>");
        html.AppendLine("    </div>");

        html.AppendLine("    <div class='info'>");
        html.AppendLine($"        <div class='info-row'><span class='info-label'>Cliente:</span><span>{prestamo.Persona.PersonaNombreCompleto}</span></div>");
        html.AppendLine($"        <div class='info-row'><span class='info-label'>Identificación:</span><span>{prestamo.Persona.PersonaIdentificacion}</span></div>");
        html.AppendLine($"        <div class='info-row'><span class='info-label'>Monto Préstamo:</span><span>{prestamo.PrestamoMonto:N2} {prestamo.Moneda.MonedaCodigo}</span></div>");
        html.AppendLine($"        <div class='info-row'><span class='info-label'>Tasa Interés:</span><span>{prestamo.PrestamoTasaInteres}% anual</span></div>");
        html.AppendLine($"        <div class='info-row'><span class='info-label'>Plazo:</span><span>{prestamo.PrestamoPlazo} cuotas ({prestamo.FrecuenciaPago.FrecuenciaPagoNombre})</span></div>");
        html.AppendLine($"        <div class='info-row'><span class='info-label'>Fecha Desembolso:</span><span>{prestamo.PrestamoFechaDesembolso:dd/MM/yyyy}</span></div>");
        html.AppendLine($"        <div class='info-row'><span class='info-label'>Fecha Vencimiento:</span><span>{prestamo.PrestamoFechaVencimiento:dd/MM/yyyy}</span></div>");
        html.AppendLine("    </div>");

        html.AppendLine("    <table>");
        html.AppendLine("        <thead>");
        html.AppendLine("            <tr>");
        html.AppendLine("                <th>#</th>");
        html.AppendLine("                <th>Fecha Pago</th>");
        html.AppendLine("                <th>Capital</th>");
        html.AppendLine("                <th>Interés</th>");
        html.AppendLine("                <th>Cuota</th>");
        html.AppendLine("                <th>Saldo Capital</th>");
        html.AppendLine("            </tr>");
        html.AppendLine("        </thead>");
        html.AppendLine("        <tbody>");

        decimal totalCapital = 0, totalInteres = 0, totalCuota = 0;

        foreach (var cuota in tablaAmortizacion)
        {
            html.AppendLine("            <tr>");
            html.AppendLine($"                <td>{cuota.NumeroCuota}</td>");
            html.AppendLine($"                <td>{cuota.FechaPago:dd/MM/yyyy}</td>");
            html.AppendLine($"                <td class='amount'>{cuota.Capital:N2}</td>");
            html.AppendLine($"                <td class='amount'>{cuota.Interes:N2}</td>");
            html.AppendLine($"                <td class='amount'><strong>{cuota.Cuota:N2}</strong></td>");
            html.AppendLine($"                <td class='amount'>{cuota.SaldoCapital:N2}</td>");
            html.AppendLine("            </tr>");

            totalCapital += cuota.Capital;
            totalInteres += cuota.Interes;
            totalCuota += cuota.Cuota;
        }

        html.AppendLine("            <tr class='total-row'>");
        html.AppendLine("                <td colspan='2'>TOTALES</td>");
        html.AppendLine($"                <td class='amount'>{totalCapital:N2}</td>");
        html.AppendLine($"                <td class='amount'>{totalInteres:N2}</td>");
        html.AppendLine($"                <td class='amount'>{totalCuota:N2}</td>");
        html.AppendLine("                <td class='amount'>-</td>");
        html.AppendLine("            </tr>");

        html.AppendLine("        </tbody>");
        html.AppendLine("    </table>");

        html.AppendLine("    <div class='summary'>");
        html.AppendLine("        <div class='summary-item'><span>Monto Prestado:</span><span>" + prestamo.PrestamoMonto.ToString("N2") + " " + prestamo.Moneda.MonedaCodigo + "</span></div>");
        html.AppendLine("        <div class='summary-item'><span>Total Intereses:</span><span>" + totalInteres.ToString("N2") + " " + prestamo.Moneda.MonedaCodigo + "</span></div>");
        html.AppendLine("        <div class='summary-item' style='font-size: 18px; font-weight: bold; border-top: 2px solid #333; padding-top: 10px;'>");
        html.AppendLine("            <span>TOTAL A PAGAR:</span><span>" + totalCuota.ToString("N2") + " " + prestamo.Moneda.MonedaCodigo + "</span>");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");

        html.AppendLine("    <div style='margin-top: 40px; font-size: 12px; color: #666; text-align: center;'>");
        html.AppendLine("        <p>Este plan de pagos es una proyección y puede variar en caso de pagos anticipados, mora o cambios en las condiciones del préstamo.</p>");
        html.AppendLine($"        <p>Para más información contacte a ALPHACREDIT - {prestamo.Sucursal.SucursalNombre}</p>");
        html.AppendLine("    </div>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    /// <summary>
    /// Registra una impresión de documento
    /// </summary>
    public async Task<PrestamoDocumento> RegistrarImpresion(long prestamoId, string tipoDocumento, string usuario, string? ip = null)
    {
        // Buscar o crear el documento
        var documento = await _context.PrestamosDocumentos
            .Include(pd => pd.Impresiones)
            .FirstOrDefaultAsync(pd => pd.PrestamoId == prestamoId && pd.PrestamoDocumentoTipo == tipoDocumento);

        if (documento == null)
        {
            documento = new PrestamoDocumento
            {
                PrestamoId = prestamoId,
                PrestamoDocumentoTipo = tipoDocumento,
                PrestamoDocumentoUserCrea = usuario,
                PrestamoDocumentoFechaCreacion = DateTime.UtcNow
            };
            _context.PrestamosDocumentos.Add(documento);
            await _context.SaveChangesAsync(); // Guardar primero para obtener el ID
        }

        // Registrar la impresión
        var impresion = new PrestamoDocumentoImpresion
        {
            PrestamoDocumentoId = documento.PrestamoDocumentoId,
            PrestamoDocumentoImpresionFecha = DateTime.UtcNow,
            PrestamoDocumentoImpresionUsuario = usuario,
            PrestamoDocumentoImpresionIp = ip
        };

        // Actualizar contadores
        documento.PrestamoDocumentoVecesImpreso++;
        documento.PrestamoDocumentoFechaUltimaImpresion = DateTime.UtcNow;

        if (documento.PrestamoDocumentoFechaPrimeraImpresion == null)
        {
            documento.PrestamoDocumentoFechaPrimeraImpresion = DateTime.UtcNow;
        }

        _context.PrestamosDocumentosImpresiones.Add(impresion);
        await _context.SaveChangesAsync(); // Guardar la impresión y actualizaciones

        return documento;
    }

    /// <summary>
    /// Obtiene el historial de impresiones de un documento
    /// </summary>
    public async Task<List<PrestamoDocumentoImpresion>> ObtenerHistorialImpresiones(long prestamoId, string tipoDocumento)
    {
        var documento = await _context.PrestamosDocumentos
            .Include(pd => pd.Impresiones)
            .FirstOrDefaultAsync(pd => pd.PrestamoId == prestamoId && pd.PrestamoDocumentoTipo == tipoDocumento);

        return documento?.Impresiones.OrderByDescending(i => i.PrestamoDocumentoImpresionFecha).ToList()
            ?? new List<PrestamoDocumentoImpresion>();
    }

    /// <summary>
    /// Convierte un número a letras (simplificado para español)
    /// </summary>
    private string NumeroALetras(decimal numero)
    {
        if (numero == 0) return "CERO";

        string[] unidades = { "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };
        string[] decenas = { "", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
        string[] centenas = { "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };
        string[] especiales = { "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE" };

        int parteEntera = (int)numero;
        int centavos = (int)((numero - parteEntera) * 100);

        string resultado = "";

        if (parteEntera >= 1000000)
        {
            int millones = parteEntera / 1000000;
            resultado += millones == 1 ? "UN MILLÓN " : ConvertirGrupo(millones) + " MILLONES ";
            parteEntera %= 1000000;
        }

        if (parteEntera >= 1000)
        {
            int miles = parteEntera / 1000;
            resultado += miles == 1 ? "MIL " : ConvertirGrupo(miles) + " MIL ";
            parteEntera %= 1000;
        }

        if (parteEntera > 0)
        {
            if (parteEntera == 100)
                resultado += "CIEN";
            else
                resultado += ConvertirGrupo(parteEntera);
        }

        if (centavos > 0)
        {
            resultado += $" CON {centavos:00}/100";
        }

        return resultado.Trim();

        string ConvertirGrupo(int num)
        {
            if (num == 0) return "";

            string res = "";
            int c = num / 100;
            int d = (num % 100) / 10;
            int u = num % 10;

            if (c > 0) res += centenas[c] + " ";

            if (d == 1 && u > 0)
                res += especiales[u] + " ";
            else
            {
                if (d > 0) res += decenas[d] + " ";
                if (u > 0) res += (d > 0 ? "Y " : "") + unidades[u] + " ";
            }

            return res.Trim();
        }
    }
}
