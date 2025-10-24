namespace AlphaCredit.Api.Models;

/// <summary>
/// Configuración de moneda del sistema
/// </summary>
public class CurrencySettings
{
    /// <summary>
    /// Código ISO de la moneda (ej: HNL, USD, EUR)
    /// </summary>
    public string Code { get; set; } = "HNL";

    /// <summary>
    /// Símbolo de la moneda (ej: L, $, €)
    /// </summary>
    public string Symbol { get; set; } = "L";

    /// <summary>
    /// Nombre de la moneda (ej: LEMPIRAS, DÓLARES, EUROS)
    /// </summary>
    public string Name { get; set; } = "LEMPIRAS";

    /// <summary>
    /// Locale para formateo (ej: es-HN, en-US, es-ES)
    /// </summary>
    public string Locale { get; set; } = "es-HN";
}
