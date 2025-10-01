using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace StoryNest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V10_AddStorySearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Stories",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "\r\n                setweight(to_tsvector('simple', coalesce(title, '')), 'A') ||\r\n                setweight(to_tsvector('simple', coalesce(content, '')), 'B') ||\r\n                setweight(to_tsvector('simple', coalesce(summary, '')), 'C') ||\r\n                setweight(to_tsvector('simple', coalesce(slug, '')), 'D')\r\n            ",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stories_SearchVector",
                table: "Stories",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stories_SearchVector",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Stories");
        }
    }
}
