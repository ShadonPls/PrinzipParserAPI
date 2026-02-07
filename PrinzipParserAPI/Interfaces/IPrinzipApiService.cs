using PrinzipParserAPI.Models;

namespace PrinzipParserAPI.Interfaces
{
    public interface IPrinzipApiService
    {
        /// <summary>
        /// Извлекает ID квартиры из пользовательской ссылки
        /// </summary>
        /// <param name="url">Ссылка на квартиру</param>
        /// <returns>ID квартиры или null, если извлечь не удалось</returns>
        int? ExtractIdFromUrl(string url);

        /// <summary>
        /// Получает актуальную информацию о квартире (цену, статус)
        /// </summary>
        /// <param name="id">ID квартиры</param>
        /// <returns>Информация о квартире или null при ошибке</returns>
        Task<ApartmentInfo?> GetApartmentInfoAsync(int id);
    }
}
