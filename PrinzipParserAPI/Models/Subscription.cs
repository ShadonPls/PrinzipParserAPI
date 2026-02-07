namespace PrinzipParserAPI.Models;

/// <summary>
/// Модель подписки пользователя на мониторинг цены квартиры
/// </summary>
public class Subscription
{
    public int Id { get; set; }

    /// <summary>
    /// Ссылка, которую предоставил пользователь
    /// </summary>
    public string UserUrl { get; set; } = string.Empty;

    /// <summary>
    /// ID квартиры, извлеченный из URL (например, 67959)
    /// </summary>
    public int ApartmentId { get; set; }

    /// <summary>
    /// Email для отправки уведомлений
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Последняя известная цена (для отслеживания изменений)
    /// </summary>
    public decimal LastPrice { get; set; }

    /// <summary>
    /// Последний известный статус ("Свободна", "Забронирована" и т.д.)
    /// </summary>
    public string LastStatus { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания подписки
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата последней проверки
    /// </summary>
    public DateTime LastCheckedAt { get; set; }
}
