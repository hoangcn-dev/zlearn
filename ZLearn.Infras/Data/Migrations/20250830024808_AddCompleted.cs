using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.Infras.Migrations
{
    public partial class AddCompleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Completed",
                table: "ExamParticipant",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Completed",
                table: "ExamParticipant");
        }
    }
}
