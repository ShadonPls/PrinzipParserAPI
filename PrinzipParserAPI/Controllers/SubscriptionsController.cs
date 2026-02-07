using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrinzipParserAPI.Data;
using PrinzipParserAPI.Interfaces;
using PrinzipParserAPI.Models;

namespace PrinzipParserAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPrinzipApiService _provider;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        AppDbContext db,
        IPrinzipApiService provider,
        ILogger<SubscriptionsController> logger)
    {
        _db = db;
        _provider = provider;
        _logger = logger;
    }

    /// <summary>
    /// Подписка на мониторинг цены квартиры
    /// </summary>
    /// <param name="url">Ссылка на квартиру</param>
    /// <param name="email">Email для уведомлений</param>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromQuery] string url, [FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { Error = "URL и Email обязательны" });
        }

        // 1. Извлекаем ID квартиры из URL
        var apartmentId = _provider.ExtractIdFromUrl(url);
        if (apartmentId == null)
        {
            return BadRequest(new { Error = "Не удалось извлечь ID квартиры из URL" });
        }

        // 2. Проверяем, существует ли квартира (делаем тестовый запрос)
        var info = await _provider.GetApartmentInfoAsync(apartmentId.Value);
        if (info == null)
        {
            return BadRequest(new { Error = "Квартира не найдена или API недоступно" });
        }

        // 3. Проверяем, нет ли уже такой подписки
        var existing = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ApartmentId == apartmentId && s.Email == email);

        if (existing != null)
        {
            return Conflict(new { Error = "Подписка уже существует", SubscriptionId = existing.Id });
        }

        // 4. Создаем подписку
        var subscription = new Subscription
        {
            UserUrl = url,
            ApartmentId = apartmentId.Value,
            Email = email,
            LastPrice = info.Price,
            LastStatus = info.Status,
            CreatedAt = DateTime.UtcNow,
            LastCheckedAt = DateTime.UtcNow
        };

        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Создана подписка {SubId} для квартиры {ApartmentId} на email {Email}",
            subscription.Id, apartmentId, email);

        return Ok(new
        {
            Message = "Подписка успешно оформлена",
            SubscriptionId = subscription.Id,
            ApartmentId = apartmentId,
            CurrentPrice = info.Price,
            CurrentStatus = info.Status
        });
    }

    /// <summary>
    /// Получить актуальные цены всех отслеживаемых квартир
    /// </summary>
    [HttpGet("prices")]
    public async Task<IActionResult> GetCurrentPrices()
    {
        var subscriptions = await _db.Subscriptions
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var result = subscriptions.Select(s => new
        {
            s.Id,
            s.ApartmentId,
            Url = s.UserUrl,
            s.Email,
            CurrentPrice = s.LastPrice,
            CurrentStatus = s.LastStatus,
            LastChecked = s.LastCheckedAt
        });

        return Ok(result);
    }

    /// <summary>
    /// Удалить подписку
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        var subscription = await _db.Subscriptions.FindAsync(id);

        if (subscription == null)
        {
            return NotFound(new { Error = "Подписка не найдена" });
        }

        _db.Subscriptions.Remove(subscription);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Удалена подписка {SubId}", id);

        return Ok(new { Message = "Подписка удалена" });
    }
}
