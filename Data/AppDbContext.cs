using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_akushevich.Models;

namespace wt_lab7_7postgres_efcore_akushevich.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Деньги храним с точностью до копеек: numeric(18,2)
        modelBuilder.Entity<BankAccount>()
            .Property(a => a.Balance)
            .HasPrecision(18, 2);

        // Начальные данные
        modelBuilder.Entity<BankAccount>().HasData(
            new BankAccount
            {
                Id = 1,
                OwnerName = "Акушевич Андрей Сергеевич",
                AccountNumber = "BY01SAV0001",
                Balance = 2000m,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new BankAccount
            {
                Id = 2,
                OwnerName = "Петрова Анна Сергеевна",
                AccountNumber = "BY02CRD0002",
                Balance = 0m,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new BankAccount
            {
                Id = 3,
                OwnerName = "Сидоров Пётр Петрович",
                AccountNumber = "BY03DEP0003",
                Balance = 5000m,
                IsActive = false,
                CreatedAt = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc)
            },
            new BankAccount
            {
                Id = 4,
                OwnerName = "Морозова Елена Викторовна",
                AccountNumber = "BY04DEP0004",
                Balance = 12000m,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)
            },
            new BankAccount
            {
                Id = 5,
                OwnerName = "Кузнецов Андрей Игоревич",
                AccountNumber = "BY05SAV0005",
                Balance = 750m,
                IsActive = false,
                CreatedAt = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}