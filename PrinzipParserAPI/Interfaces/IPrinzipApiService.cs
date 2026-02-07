using PrinzipParserAPI.Models;

namespace PrinzipParserAPI.Interfaces
{
    /// <summary>
    /// Контракт для работы с данными квартир из любого источника
    /// </summary>
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
