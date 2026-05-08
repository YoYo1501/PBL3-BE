using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ReceiptCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receipts_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_InvoiceId",
                table: "Receipts",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ReceiptCode",
                table: "Receipts",
                column: "ReceiptCode",
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO Receipts (InvoiceId, ReceiptCode, PaidAmount, PaidAt, PaymentMethod, TransactionCode, Status, CreatedAt)
                SELECT
                    i.Id AS InvoiceId,
                    CONCAT(
                        'BL-',
                        REPLACE(REPLACE(REPLACE(i.Period, '-', ''), '/', ''), ' ', ''),
                        '-',
                        RIGHT(CONCAT('000000', CAST(i.Id AS varchar(20))), 6)
                    ) AS ReceiptCode,
                    i.TotalAmount AS PaidAmount,
                    COALESCE(i.PaidAt, i.IssuedAt) AS PaidAt,
                    COALESCE(NULLIF(i.PaymentMethod, ''), 'Unknown') AS PaymentMethod,
                    COALESCE(i.TransactionCode, '') AS TransactionCode,
                    'Success' AS Status,
                    GETUTCDATE() AS CreatedAt
                FROM Invoices i
                WHERE i.Status = 'Paid'
                  AND NOT EXISTS (SELECT 1 FROM Receipts r WHERE r.InvoiceId = i.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Receipts");
        }
    }
}
