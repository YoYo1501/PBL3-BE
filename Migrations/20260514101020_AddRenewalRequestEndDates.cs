using System;
using BackendAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260514101020_AddRenewalRequestEndDates")]
    public partial class AddRenewalRequestEndDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDateAfterRenewal",
                table: "RenewalRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDateBeforeRenewal",
                table: "RenewalRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("""
                UPDATE rr
                SET
                    ContractEndDateBeforeRenewal =
                        CASE
                            WHEN rr.Status = 'Approved' THEN DATEADD(month, -rp.DurationMonths, c.EndDate)
                            ELSE c.EndDate
                        END,
                    ContractEndDateAfterRenewal =
                        CASE
                            WHEN rr.Status = 'Approved' THEN c.EndDate
                            ELSE DATEADD(month, rp.DurationMonths, c.EndDate)
                        END
                FROM RenewalRequests rr
                INNER JOIN Contracts c ON rr.ContractId = c.Id
                INNER JOIN RenewalPackages rp ON rr.RenewalPackageId = rp.Id
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractEndDateAfterRenewal",
                table: "RenewalRequests");

            migrationBuilder.DropColumn(
                name: "ContractEndDateBeforeRenewal",
                table: "RenewalRequests");
        }
    }
}
