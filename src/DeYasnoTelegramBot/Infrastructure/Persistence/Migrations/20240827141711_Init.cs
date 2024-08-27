using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeYasnoTelegramBot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscribers",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    BrowserSessionId = table.Column<string>(type: "text", nullable: true),
                    InputStep = table.Column<int>(type: "integer", nullable: false),
                    UserRegion = table.Column<string>(type: "text", nullable: true),
                    UserCity = table.Column<string>(type: "text", nullable: true),
                    UserStreet = table.Column<string>(type: "text", nullable: true),
                    UserHouseNumber = table.Column<string>(type: "text", nullable: true),
                    OutageSchedules = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscribers", x => x.ChatId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscribers");
        }
    }
}
