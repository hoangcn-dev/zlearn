using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.Infras.Data.Migrations
{
    public partial class AddMediaFileDuration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Width",
                table: "MediaFiles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Height",
                table: "MediaFiles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<double>(
                name: "SecDuration",
                table: "MediaFiles",
                type: "double precision",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecDuration",
                table: "MediaFiles");

            migrationBuilder.AlterColumn<int>(
                name: "Width",
                table: "MediaFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Height",
                table: "MediaFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
