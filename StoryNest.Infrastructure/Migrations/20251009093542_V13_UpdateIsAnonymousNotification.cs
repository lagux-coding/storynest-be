using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V13_UpdateIsAnonymousNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "Notifications");
        }
    }
}
