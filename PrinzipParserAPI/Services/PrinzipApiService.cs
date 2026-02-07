
using PrinzipParserAPI.Models;
using PrinzipParserAPI.Interfaces;
using System.Text.RegularExpressions;

namespace PrinzipParserAPI.Services;

/// <summary>
/// Реализация провайдера данных для Prinzip.su через их публичный API
/// </summary>
public class PrinzipApiService : IPrinzipApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PrinzipApiService> _logger;

    public PrinzipApiService(HttpClient httpClient, ILogger<PrinzipApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Прикидываемся браузером (на случай защиты от ботов)
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public int? ExtractIdFromUrl(string url)
    {
        // Паттерн для извлечения числового ID из URL
        // Примеры URL:
        // https://prinzip.su/apartments/park_kultury/67959/
        // https://prinzip.su/apartments/67959
        // /apartments/67959/

        var match = Regex.Match(url, @"/(\d+)/?$");

        if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
        {
            _logger.LogInformation("Извлечен ID {Id} из URL {Url}", id, url);
            return id;
        }

        _logger.LogWarning("Не удалось извлечь ID из URL {Url}", url);
        return null;
    }

    public async Task<ApartmentInfo?> GetApartmentInfoAsync(int id)
    {
        var apiUrl = $"https://prinzip.su/api/v1/public/apartments/{id}";

        try
        {
            _logger.LogDebug("Запрос к API: {Url}", apiUrl);

            var dto = await _httpClient.GetFromJsonAsync<PrinzipApartmentDto>(apiUrl);

            if (dto == null)
            {
                _logger.LogWarning("API вернул null для квартиры {Id}", id);
                return null;
            }

            // Берем цену при полной оплате (payment_method = "full")
            var fullPaymentPricing = dto.Pricings.FirstOrDefault(p => p.PaymentMethod == "full");

            if (fullPaymentPricing == null)
            {
                _logger.LogWarning("Не найдена цена для метода оплаты 'full' у квартиры {Id}", id);
                return null;
            }

            var info = new ApartmentInfo
            {
                Id = dto.Id,
                Price = fullPaymentPricing.Price,
                Status = dto.Status
            };

            _logger.LogInformation("Получена информация о квартире {Id}: цена {Price}, статус {Status}",
                id, info.Price, info.Status);

            return info;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP при запросе квартиры {Id}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при получении данных квартиры {Id}", id);
            return null;
        }
    }
}
