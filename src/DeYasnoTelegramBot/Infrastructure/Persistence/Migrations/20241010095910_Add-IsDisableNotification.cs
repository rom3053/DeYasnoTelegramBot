using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeYasnoTelegramBot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDisableNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisableNotification",
                table: "Subscribers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisableNotification",
                table: "Subscribers");
        }
    }
}
