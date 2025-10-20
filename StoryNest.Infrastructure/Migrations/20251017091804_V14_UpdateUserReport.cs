using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V14_UpdateUserReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_Stories_reported_story_id",
                table: "UserReports");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_Users_UserId",
                table: "UserReports");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_Users_reported_id",
                table: "UserReports");

            migrationBuilder.DropIndex(
                name: "IX_UserReports_UserId",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserReports");

            migrationBuilder.AlterColumn<int>(
                name: "reported_story_id",
                table: "UserReports",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<long>(
                name: "ReporterId",
                table: "UserReports",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "reported_comment_id",
                table: "UserReports",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_reported_comment_id",
                table: "UserReports",
                column: "reported_comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_ReporterId",
                table: "UserReports",
                column: "ReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_Comments_reported_comment_id",
                table: "UserReports",
                column: "reported_comment_id",
                principalTable: "Comments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_Stories_reported_story_id",
                table: "UserReports",
                column: "reported_story_id",
                principalTable: "Stories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_Users_ReporterId",
                table: "UserReports",
                column: "ReporterId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_Users_reported_id",
                table: "UserReports",
                column: "reported_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_Comments_reported_comment_id",
                table: "UserReports");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_Stories_reported_story_id",
                table: "UserReports");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_Users_ReporterId",
                table: "UserReports");

            migrationBuilder.DropForeignKey(
                name: "FK_UserReports_Users_reported_id",
                table: "UserReports");

            migrationBuilder.DropIndex(
                name: "IX_UserReports_reported_comment_id",
                table: "UserReports");

            migrationBuilder.DropIndex(
                name: "IX_UserReports_ReporterId",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "ReporterId",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "reported_comment_id",
                table: "UserReports");

            migrationBuilder.AlterColumn<int>(
                name: "reported_story_id",
                table: "UserReports",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "UserReports",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_UserId",
                table: "UserReports",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_Stories_reported_story_id",
                table: "UserReports",
                column: "reported_story_id",
                principalTable: "Stories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_Users_UserId",
                table: "UserReports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserReports_Users_reported_id",
                table: "UserReports",
                column: "reported_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
