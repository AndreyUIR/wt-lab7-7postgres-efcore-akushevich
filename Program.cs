using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_akushevich.Data;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text.Unicode;
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
        Technology = "ASP.NET Core Minimal API",
        Version = "1.1",
        Description = "REST API банковских счетов"
    };
})
.WithTags("Info")
.WithSummary("Информация о приложении");

app.MapGet("/api/accounts", (string? search) =>
{
    var accounts = BankAccountStore.Accounts.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        accounts = accounts.Where(a =>
            a.OwnerName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            a.AccountNumber.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    return Results.Ok(accounts.ToList());
})
.WithTags("Accounts")
.WithSummary("Получить список счетов (с поиском по параметру search)")
.Produces<List<BankAccount>>(StatusCodes.Status200OK);

app.MapGet("/api/accounts/{id:int}", (int id) =>
{
    var account = BankAccountStore.Accounts.FirstOrDefault(a => a.Id == id);

    if (account == null)
        return Results.NotFound();

    return Results.Ok(account);
})
.WithTags("Accounts")
.WithSummary("Получить счёт по идентификатору")
.Produces<BankAccount>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/api/accounts", (BankAccount account) =>
{
    var errors = ValidateAccount(account);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    account.Id = BankAccountStore.Accounts.Count == 0
        ? 1
        : BankAccountStore.Accounts.Max(a => a.Id) + 1;

    BankAccountStore.Accounts.Add(account);

    return Results.Created($"/api/accounts/{account.Id}", account);
})
.WithTags("Accounts")
.WithSummary("Создать счёт")
.Produces<BankAccount>(StatusCodes.Status201Created)
.ProducesValidationProblem();

app.MapPut("/api/accounts/{id:int}", (int id, BankAccount updated) =>
{
    var account = BankAccountStore.Accounts.FirstOrDefault(a => a.Id == id);

    if (account == null)
        return Results.NotFound();

    var errors = ValidateAccount(updated);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    account.OwnerName = updated.OwnerName;
    account.AccountNumber = updated.AccountNumber;
    account.Balance = updated.Balance;
    account.IsActive = updated.IsActive;

    return Results.Ok(account);
})
.WithTags("Accounts")
.WithSummary("Изменить счёт")
.Produces<BankAccount>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.ProducesValidationProblem();

app.MapDelete("/api/accounts/{id:int}", (int id) =>
{
    var account = BankAccountStore.Accounts.FirstOrDefault(a => a.Id == id);

    if (account == null)
        return Results.NotFound();

    BankAccountStore.Accounts.Remove(account);

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