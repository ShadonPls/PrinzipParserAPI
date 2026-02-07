using Microsoft.AspNetCore.Mvc;
using PrinzipParserAPI.Services;

namespace PrinzipParserAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServerTestController : ControllerBase
{
    /// <summary>
    /// Получить текущее значение count
    /// </summary>
    [HttpGet("count")]
    public IActionResult GetCount()
    {
        var count = Server.GetCount();
        return Ok(new { Count = count });
    }

    /// <summary>
    /// Добавить значение к count
    /// </summary>
    [HttpPost("add")]
    public IActionResult AddToCount([FromQuery] int value)
    {
        Server.AddToCount(value);
        var newCount = Server.GetCount();
        return Ok(new { Message = $"Добавлено {value}", NewCount = newCount });
    }

    /// <summary>
    /// Нагрузочный тест: 100 параллельных читателей и 10 писателей
    /// </summary>
    [HttpPost("stress-test")]
    public async Task<IActionResult> StressTest()
    {
        var tasks = new List<Task>();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 1000; j++)
                {
                    var _ = Server.GetCount();
                }
            }));
        }

        for (int i = 0; i < 10; i++)
        {
            var capturedI = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    Server.AddToCount(1);
                    Thread.Sleep(1);
                }
            }));
        }

        await Task.WhenAll(tasks);

        var finalCount = Server.GetCount();
        return Ok(new
        {
            Message = "Стресс-тест завершен",
            ExpectedCount = 10 * 100,
            ActualCount = finalCount,
            Success = finalCount == 1000
        });
    }
}
