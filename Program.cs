using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_akushevich.Data;
using wt_lab7_7postgres_efcore_akushevich.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = BuildConnectionString(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Encoder =
        JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic);
});

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Bank Accounts API v1");
    });
}

app.MapGet("/", () => "Minimal API работает!");

app.MapGet("/api/info", () =>
{
    return new
    {
        Application = "Bank Accounts API",
        Technology = "ASP.NET Core Minimal API + EF Core + PostgreSQL",
        Version = "2.0"
    };
})
.WithTags("Info")
.WithSummary("Информация о приложении");

// ---------- Получение списка ----------
app.MapGet("/api/accounts", async (AppDbContext db) =>
{
    var accounts = await db.BankAccounts.OrderBy(a => a.Id).ToListAsync();
    return Results.Ok(accounts);
})
.WithTags("Accounts")
.WithSummary("Получить список счетов")
.Produces<List<BankAccount>>(StatusCodes.Status200OK);

// ---------- Получение по идентификатору ----------
app.MapGet("/api/accounts/{id:int}", async (int id, AppDbContext db) =>
{
    var account = await db.BankAccounts.FirstOrDefaultAsync(a => a.Id == id);

    if (account == null)
        return Results.NotFound();

    return Results.Ok(account);
})
.WithTags("Accounts")
.WithSummary("Получить счёт по идентификатору")
.Produces<BankAccount>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// ---------- Поиск (LINQ → SQL) ----------
app.MapGet("/api/accounts/search", async (string? search, AppDbContext db) =>
{
    var query = db.BankAccounts.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var text = search.ToLower();
        query = query.Where(a =>
            a.OwnerName.ToLower().Contains(text) ||
            a.AccountNumber.ToLower().Contains(text));
    }

    var accounts = await query.OrderBy(a => a.Id).ToListAsync();
    return Results.Ok(accounts);
})
.WithTags("Accounts")
.WithSummary("Поиск счетов по владельцу или номеру")
.Produces<List<BankAccount>>(StatusCodes.Status200OK);

// ---------- LINQ 1. Фильтрация: баланс больше порога ----------
app.MapGet("/api/accounts/rich", async (decimal? min, AppDbContext db) =>
{
    var threshold = min ?? 1000m;

    var accounts = await db.BankAccounts
        .Where(a => a.Balance > threshold)
        .ToListAsync();

    return Results.Ok(accounts);
})
.WithTags("LINQ")
.WithSummary("Счета с балансом больше порога (по умолчанию 1000)");

// ---------- LINQ 2. Сортировка по балансу ----------
app.MapGet("/api/accounts/sorted", async (AppDbContext db) =>
{
    var accounts = await db.BankAccounts
        .OrderBy(a => a.Balance)
        .ToListAsync();

    return Results.Ok(accounts);
})
.WithTags("LINQ")
.WithSummary("Счета по возрастанию баланса");

// ---------- LINQ 3. Выборка с условием и сортировкой ----------
app.MapGet("/api/accounts/active", async (AppDbContext db) =>
{
    var accounts = await db.BankAccounts
        .Where(a => a.IsActive)
        .OrderBy(a => a.OwnerName)
        .ToListAsync();

    return Results.Ok(accounts);
})
.WithTags("LINQ")
.WithSummary("Активные счета по алфавиту владельцев");

// ---------- LINQ 4 (собственный). Статистика по счетам ----------
app.MapGet("/api/accounts/stats", async (AppDbContext db) =>
{
    var total = await db.BankAccounts.CountAsync();
    var active = await db.BankAccounts.CountAsync(a => a.IsActive);
    var totalBalance = await db.BankAccounts.SumAsync(a => a.Balance);
    var average = await db.BankAccounts.AverageAsync(a => (decimal?)a.Balance);

    return Results.Ok(new
    {
        total,
        active,
        totalBalance,
        averageBalance = average ?? 0m
    });
})
.WithTags("LINQ")
.WithSummary("Количество счетов, сумма и средний баланс");

// ---------- Создание ----------
app.MapPost("/api/accounts", async (BankAccount account, AppDbContext db) =>
{
    var errors = ValidateAccount(account);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    account.Id = 0; // идентификатор выдаёт база, значение клиента игнорируем
    account.CreatedAt = DateTime.UtcNow;

    db.BankAccounts.Add(account);
    await db.SaveChangesAsync();

    return Results.Created($"/api/accounts/{account.Id}", account);
})
.WithTags("Accounts")
.WithSummary("Создать счёт")
.Produces<BankAccount>(StatusCodes.Status201Created)
.ProducesValidationProblem();

// ---------- Изменение ----------
app.MapPut("/api/accounts/{id:int}", async (int id, BankAccount updated, AppDbContext db) =>
{
    var account = await db.BankAccounts.FirstOrDefaultAsync(a => a.Id == id);

    if (account == null)
        return Results.NotFound();

    var errors = ValidateAccount(updated);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    account.OwnerName = updated.OwnerName;
    account.AccountNumber = updated.AccountNumber;
    account.Balance = updated.Balance;
    account.IsActive = updated.IsActive;

    await db.SaveChangesAsync();

    return Results.Ok(account);
})
.WithTags("Accounts")
.WithSummary("Изменить счёт")
.Produces<BankAccount>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.ProducesValidationProblem();

// ---------- Удаление ----------
app.MapDelete("/api/accounts/{id:int}", async (int id, AppDbContext db) =>
{
    var account = await db.BankAccounts.FirstOrDefaultAsync(a => a.Id == id);

    if (account == null)
        return Results.NotFound();

    db.BankAccounts.Remove(account);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithTags("Accounts")
.WithSummary("Удалить счёт")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

var port = Environment.GetEnvironmentVariable("PORT");

if (port != null)
{
    // Render / Docker: слушаем все интерфейсы на заданном порту
    app.Run($"http://0.0.0.0:{port}");
}
else
{
    // Локальный dotnet run: порты из launchSettings.json
    app.Run();
}

static Dictionary<string, string[]> ValidateAccount(BankAccount account)
{
    var results = new List<ValidationResult>();

    Validator.TryValidateObject(
        account,
        new ValidationContext(account),
        results,
        validateAllProperties: true);

    return results
        .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
        .ToDictionary(
            g => g.Key,
            g => g.Select(r => r.ErrorMessage ?? "Ошибка валидации").ToArray());
}
static string BuildConnectionString(IConfiguration configuration)
{
    var databaseUrl = configuration["DATABASE_URL"];

    // Локальный запуск: строка из appsettings
    if (string.IsNullOrWhiteSpace(databaseUrl))
    {
        return configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Не найдена строка подключения DefaultConnection и переменная DATABASE_URL.");
    }

    // Render: разбираем URL вида postgresql://user:password@host[:port]/database
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var port = uri.Port > 0 ? uri.Port : 5432;

    return
        $"Host={uri.Host};" +
        $"Port={port};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={Uri.UnescapeDataString(userInfo[0])};" +
        $"Password={Uri.UnescapeDataString(userInfo[1])};" +
        "SSL Mode=Disable";
}