using Microsoft.EntityFrameworkCore;
using PrinzipParserAPI.Data;
using PrinzipParserAPI.Services;
using PrinzipParserAPI.Workers;
using PrinzipParserAPI.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════
// 1. База данных (MS SQL)
// ═══════════════════════════════════════════════════════════
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// ═══════════════════════════════════════════════════════════
// 2. Сервисы (провайдер данных квартир)
// ═══════════════════════════════════════════════════════════

// Для продакшена (реальный API Prinzip.su)
builder.Services.AddHttpClient<IPrinzipApiService, PrinzipApiService>();

// Для тестирования (mock-данные без реального API)
// Раскомментируй эту строку и закомментируй предыдущую для локальной разработки:
// builder.Services.AddScoped<IApartmentDataProvider, FakeApartmentProvider>();

// ═══════════════════════════════════════════════════════════
// 3. Фоновые задачи (Workers)
// ═══════════════════════════════════════════════════════════
builder.Services.AddHostedService<PriceMonitorWorker>();

// ═══════════════════════════════════════════════════════════
// 4. Контроллеры и API
// ═══════════════════════════════════════════════════════════
builder.Services.AddControllers();

// Swagger для тестирования API (опционально, но удобно)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ═══════════════════════════════════════════════════════════
// Middleware pipeline
// ═══════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
