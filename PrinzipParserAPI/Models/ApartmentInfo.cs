namespace PrinzipParserAPI.Models;

/// <summary>
/// Универсальная модель данных о квартире (независима от конкретного API)
/// Используется для абстрагирования от Prinzip-специфичных структур
/// </summary>
public class ApartmentInfo
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
}
