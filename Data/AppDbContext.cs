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
    }
}