using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.Infras.Data.Migrations
{
    public partial class AddMediaFileWidthHeight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "MediaFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "MediaFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "MediaFiles");
        }
    }
}
