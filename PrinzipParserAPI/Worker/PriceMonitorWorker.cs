using Microsoft.EntityFrameworkCore;
using PrinzipParserAPI.Data;
using PrinzipParserAPI.Interfaces;

namespace PrinzipParserAPI.Workers;

/// <summary>
/// Фоновый сервис для периодической проверки изменения цен
/// Запускается автоматически при старте приложения
/// </summary>
public class PriceMonitorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PriceMonitorWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Интервал проверки

    public PriceMonitorWorker(
        IServiceProvider serviceProvider,
        ILogger<PriceMonitorWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PriceMonitorWorker запущен. Интервал проверки: {Interval}", _checkInterval);

        // Небольшая задержка при старте, чтобы приложение успело полностью инициализироваться
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckPricesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка в цикле мониторинга");
            }

            // Ждем до следующей проверки
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("PriceMonitorWorker остановлен");
    }

    private async Task CheckPricesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Начало проверки цен...");

        // BackgroundService - Singleton, а DbContext - Scoped
        // Нужно создавать scope вручную для каждой операции
        using var scope = _serviceProvider.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var provider = scope.ServiceProvider.GetRequiredService<IPrinzipApiService>();

        var subscriptions = await db.Subscriptions.ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
        {
            _logger.LogInformation("Нет активных подписок для проверки");
            return;
        }

        _logger.LogInformation("Найдено подписок для проверки: {Count}", subscriptions.Count);

        foreach (var sub in subscriptions)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Задержка между запросами, чтобы не перегружать API
                await Task.Delay(1000, cancellationToken);

                var info = await provider.GetApartmentInfoAsync(sub.ApartmentId);

                if (info == null)
                {
                    _logger.LogWarning("Не удалось получить данные для подписки {SubId} (квартира {ApartmentId})",
                        sub.Id, sub.ApartmentId);
                    continue;
                }

                bool hasChanges = false;

                // Проверка изменения цены
                if (info.Price != sub.LastPrice && sub.LastPrice != 0)
                {
                    _logger.LogInformation(
                        "Изменение цены для подписки {SubId}: {OldPrice} → {NewPrice}",
                        sub.Id, sub.LastPrice, info.Price);

                    // TODO: Здесь должна быть отправка Email
                    await SendPriceChangeEmailAsync(sub.Email, sub.UserUrl, sub.LastPrice, info.Price);

                    sub.LastPrice = info.Price;
                    hasChanges = true;
                }
                else if (sub.LastPrice == 0)
                {
                    // Первая проверка - просто сохраняем текущую цену
                    sub.LastPrice = info.Price;
                    hasChanges = true;
                }

                // Проверка изменения статуса
                if (!string.IsNullOrEmpty(info.Status) && info.Status != sub.LastStatus)
                {
                    _logger.LogInformation(
                        "Изменение статуса для подписки {SubId}: '{OldStatus}' → '{NewStatus}'",
                        sub.Id, sub.LastStatus, info.Status);

                    // TODO: Отправка Email об изменении статуса
                    await SendStatusChangeEmailAsync(sub.Email, sub.UserUrl, sub.LastStatus, info.Status);

                    sub.LastStatus = info.Status;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    sub.LastCheckedAt = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке подписки {SubId}", sub.Id);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Проверка цен завершена");
    }

    // Заглушки для отправки Email (в реальном проекте здесь SMTP или SendGrid)
    private Task SendPriceChangeEmailAsync(string email, string url, decimal oldPrice, decimal newPrice)
    {
        _logger.LogWarning(
            "[EMAIL] To: {Email} | Цена изменилась: {OldPrice:N0} ₽ → {NewPrice:N0} ₽ | {Url}",
            email, oldPrice, newPrice, url);
        return Task.CompletedTask;
    }

    private Task SendStatusChangeEmailAsync(string email, string url, string oldStatus, string newStatus)
    {
        _logger.LogWarning(
            "[EMAIL] To: {Email} | Статус изменился: '{OldStatus}' → '{NewStatus}' | {Url}",
            email, oldStatus, newStatus, url);
        return Task.CompletedTask;
    }
}
