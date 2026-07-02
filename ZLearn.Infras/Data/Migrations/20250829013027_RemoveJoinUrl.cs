using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.V2.Infas.Migrations
{
    public partial class RemoveJoinUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoinUrl",
                table: "Exam");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
              
            migrationBuilder.AddColumn<string>(
                name: "JoinUrl",
                table: "Exam",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
