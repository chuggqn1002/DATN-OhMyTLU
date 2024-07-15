using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OhMyTLU.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSessionForMultipleDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "UserSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "UserSessions");
        }
    }
}
