using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoryNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V16_CreateStorySentimentAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorySentimentAnalysis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<float>(type: "real", precision: 4, scale: 3, nullable: false),
                    magnitude = table.Column<float>(type: "real", precision: 6, scale: 3, nullable: false),
                    analyzed_text = table.Column<string>(type: "text", nullable: true),
                    analyzed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    job_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorySentimentAnalysis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorySentimentAnalysis_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorySentimentAnalysis_StoryId",
                table: "StorySentimentAnalysis",
                column: "StoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorySentimentAnalysis");
        }
    }
}
