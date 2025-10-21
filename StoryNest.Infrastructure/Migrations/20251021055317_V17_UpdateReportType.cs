using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoryNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V17_UpdateReportType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "UserReports",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserReports");
        }
    }
}
