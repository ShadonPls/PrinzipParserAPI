using System.Text.Json.Serialization;

namespace PrinzipParserAPI.Models;
/// <summary>
/// Информация о ценах на квартиру (может быть несколько вариантов оплаты)
/// </summary>
public class PrinzipPricing
{
    [JsonPropertyName("payment_method")]
    public string PaymentMethod { get; set; } = string.Empty; // "full", "mortgage"

    [JsonPropertyName("price_base")]
    public string PriceBaseString { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string PriceString { get; set; } = string.Empty;

    /// <summary>
    /// Конвертация строкового представления цены в decimal
    /// API отдает "3839242.00" (с точкой), парсим аккуратно
    /// </summary>
    [JsonIgnore]
    public decimal Price
    {
        get
        {
            if (decimal.TryParse(PriceString, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return 0;
        }
    }
}