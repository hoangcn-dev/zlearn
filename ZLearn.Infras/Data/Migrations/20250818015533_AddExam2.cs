using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.Infras.Migrations
{
    public partial class AddExam2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MixAnswers",
                table: "Exam",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MixQuestions",
                table: "Exam",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MixAnswers",
                table: "Exam");

            migrationBuilder.DropColumn(
                name: "MixQuestions",
                table: "Exam");
        }
    }
}
