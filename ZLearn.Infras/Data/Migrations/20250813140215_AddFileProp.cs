using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.V2.Infas.Migrations
{
    public partial class AddFileProp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsing",
                table: "MediaFiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_SourceUrl",
                table: "MediaFiles",
                column: "SourceUrl",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_SourceUrl",
                table: "MediaFiles");
            

            migrationBuilder.DropColumn(
                name: "IsUsing",
                table: "MediaFiles");

        }
    }
}
