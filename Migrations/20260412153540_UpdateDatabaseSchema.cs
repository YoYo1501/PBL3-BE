using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Students_StudentId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_RenewalRequests_Contracts_ContractId",
                table: "RenewalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RenewalRequests_RenewalPackages_RenewalPackageId",
                table: "RenewalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RenewalRequests_Students_StudentId",
                table: "RenewalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRequests_Students_StudentId",
                table: "StudentRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Students_StudentId",
                table: "Contracts",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RenewalRequests_Contracts_ContractId",
                table: "RenewalRequests",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RenewalRequests_RenewalPackages_RenewalPackageId",
                table: "RenewalRequests",
                column: "RenewalPackageId",
                principalTable: "RenewalPackages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RenewalRequests_Students_StudentId",
                table: "RenewalRequests",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRequests_Students_StudentId",
                table: "StudentRequests",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Students_StudentId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_RenewalRequests_Contracts_ContractId",
                table: "RenewalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RenewalRequests_RenewalPackages_RenewalPackageId",
                table: "RenewalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RenewalRequests_Students_StudentId",
                table: "RenewalRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRequests_Students_StudentId",
                table: "StudentRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Students_StudentId",
                table: "Contracts",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RenewalRequests_Contracts_ContractId",
                table: "RenewalRequests",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RenewalRequests_RenewalPackages_RenewalPackageId",
                table: "RenewalRequests",
                column: "RenewalPackageId",
                principalTable: "RenewalPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RenewalRequests_Students_StudentId",
                table: "RenewalRequests",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRequests_Students_StudentId",
                table: "StudentRequests",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
