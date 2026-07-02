using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.V2.Infas.Migrations
{
    public partial class AddSelectedAnswers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedAnswers",
                table: "ExamParticipant",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedAnswers",
                table: "ExamParticipant");
        }
    }
}
