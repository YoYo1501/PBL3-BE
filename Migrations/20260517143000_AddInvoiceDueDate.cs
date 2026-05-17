using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Invoices
                SET DueDate = DATEADD(SECOND, -1, DATEADD(DAY, 1, TRY_CONVERT(datetime2, CONCAT([Period], '-15'))))
                WHERE DueDate IS NULL
                  AND TRY_CONVERT(datetime2, CONCAT([Period], '-15')) IS NOT NULL
                """);

            migrationBuilder.Sql("""
                UPDATE Invoices
                SET DueDate = DATEADD(SECOND, -1, DATEADD(DAY, 1, TRY_CONVERT(datetime2, CONCAT(RIGHT([Period], 4), '-', LEFT([Period], 2), '-15'))))
                WHERE DueDate IS NULL
                  AND [Period] LIKE '[0-1][0-9]/[1-2][0-9][0-9][0-9]'
                  AND TRY_CONVERT(datetime2, CONCAT(RIGHT([Period], 4), '-', LEFT([Period], 2), '-15')) IS NOT NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Invoices");
        }
    }
}
