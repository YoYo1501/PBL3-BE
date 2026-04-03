using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SemesterId",
                table: "RoomTransferRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RoomTransferRequests_SemesterId",
                table: "RoomTransferRequests",
                column: "SemesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomTransferRequests_SemesterPeriods_SemesterId",
                table: "RoomTransferRequests",
                column: "SemesterId",
                principalTable: "SemesterPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomTransferRequests_SemesterPeriods_SemesterId",
                table: "RoomTransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_RoomTransferRequests_SemesterId",
                table: "RoomTransferRequests");

            migrationBuilder.DropColumn(
                name: "SemesterId",
                table: "RoomTransferRequests");
        }
    }
}
