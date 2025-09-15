using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V4_DatabaseChangeAIUsageLogsTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ai_usage_logs_Users_user_id",
                table: "ai_usage_logs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ai_usage_logs",
                table: "ai_usage_logs");

            migrationBuilder.RenameTable(
                name: "ai_usage_logs",
                newName: "AIUsageLogs");

            migrationBuilder.RenameIndex(
                name: "IX_ai_usage_logs_user_id",
                table: "AIUsageLogs",
                newName: "IX_AIUsageLogs_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AIUsageLogs",
                table: "AIUsageLogs",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AIUsageLogs_Users_user_id",
                table: "AIUsageLogs",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AIUsageLogs_Users_user_id",
                table: "AIUsageLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AIUsageLogs",
                table: "AIUsageLogs");

            migrationBuilder.RenameTable(
                name: "AIUsageLogs",
                newName: "ai_usage_logs");

            migrationBuilder.RenameIndex(
                name: "IX_AIUsageLogs_user_id",
                table: "ai_usage_logs",
                newName: "IX_ai_usage_logs_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ai_usage_logs",
                table: "ai_usage_logs",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ai_usage_logs_Users_user_id",
                table: "ai_usage_logs",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
