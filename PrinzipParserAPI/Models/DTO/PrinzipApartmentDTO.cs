using System.Text.Json.Serialization;

namespace PrinzipParserAPI.Models;

/// <summary>
/// DTO для ответа от Prinzip.su API
/// Содержит только необходимые поля (полный JSON гораздо больше)
/// </summary>
public class PrinzipApartmentDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("pricings")]
    public List<PrinzipPricing> Pricings { get; set; } = new();
}


