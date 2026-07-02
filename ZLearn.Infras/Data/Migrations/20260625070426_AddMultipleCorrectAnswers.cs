using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.V2.Infas.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleCorrectAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "Answers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                UPDATE ""Answers"" a
                SET ""IsCorrect"" = TRUE
                FROM ""Questions"" q
                WHERE a.""QuestionId"" = q.""Id"" AND a.""Key"" = q.""CorrectKey"";
            ");

            migrationBuilder.DropColumn(
                name: "CorrectKey",
                table: "Questions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CorrectKey",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE ""Questions"" q
                SET ""CorrectKey"" = a.""Key""
                FROM ""Answers"" a
                WHERE q.""Id"" = a.""QuestionId"" AND a.""IsCorrect"" = TRUE;
            ");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "Answers");
        }
    }
}
