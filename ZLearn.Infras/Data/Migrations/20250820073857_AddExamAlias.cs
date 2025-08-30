using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.Infras.Migrations
{
    public partial class AddExamAlias : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "Exam",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Exam_Alias",
                table: "Exam",
                column: "Alias",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exam_Alias",
                table: "Exam");

            migrationBuilder.DropColumn(
                name: "Alias",
                table: "Exam");
        }
    }
}
