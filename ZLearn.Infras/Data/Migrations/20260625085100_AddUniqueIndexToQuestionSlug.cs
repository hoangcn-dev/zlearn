using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.Infras.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToQuestionSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Questions""
                SET ""Slug"" = ""Slug"" || '-' || SUBSTRING(MD5(""Id""::text) FROM 1 FOR 8)
                WHERE ""Slug"" IN (
                    SELECT ""Slug""
                    FROM ""Questions""
                    GROUP BY ""Slug""
                    HAVING COUNT(""Slug"") > 1
                );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Slug",
                table: "Questions",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_Slug",
                table: "Questions");
        }
    }
}
