namespace PrinzipParserAPI.Services;

/// <summary>
/// Статический сервер с переменной count и методами чтения/записи
/// Использует ReaderWriterLockSlim для обеспечения:
/// - Параллельного чтения
/// - Последовательной записи
/// - Блокировки чтения во время записи
/// </summary>
public static class Server
{
    private static int _count = 0;
    private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    /// <summary>
    /// Чтение значения count.
    /// Несколько потоков могут читать одновременно
    /// Блокируется только если идет запись.
    /// </summary>
    public static int GetCount()
    {
        _lock.EnterReadLock();
        try
        {
            return _count;
        }
        finally
        {
            // Всегда освобождаем лок в finally, даже если было исключение
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Добавление значения к count.
    /// Только один поток может писать в момент времени.
    /// Блокирует всех читателей на время записи.
    /// </summary>
    public static void AddToCount(int value)
    {
        _lock.EnterWriteLock();
        try
        {
            _count += value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}
