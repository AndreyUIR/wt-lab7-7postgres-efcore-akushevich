using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace wt_lab7_7postgres_efcore_akushevich.Migrations
{
    /// <inheritdoc />
    public partial class SeedBankAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BankAccounts",
                columns: new[] { "Id", "AccountNumber", "Balance", "CreatedAt", "IsActive", "OwnerName" },
                values: new object[,]
                {
                    { 1, "BY01SAV0001", 2000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Акушевич Андрей Сергеевич" },
                    { 2, "BY02CRD0002", 0m, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), true, "Петрова Анна Сергеевна" },
                    { 3, "BY03DEP0003", 5000m, new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), false, "Сидоров Пётр Петрович" },
                    { 4, "BY04DEP0004", 12000m, new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), true, "Морозова Елена Викторовна" },
                    { 5, "BY05SAV0005", 750m, new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Utc), false, "Кузнецов Андрей Игоревич" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
