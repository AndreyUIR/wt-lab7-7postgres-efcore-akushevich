using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_akushevich.Data;
using wt_lab7_7postgres_efcore_akushevich.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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

// ---------- Создание ----------
app.MapPost("/api/accounts", async (BankAccount account, AppDbContext db) =>
{
    var errors = ValidateAccount(account);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    account.Id = 0; // идентификатор выдаёт база, значение клиента игнорируем

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