using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZLearn.Infras.Data.Migrations
{
    public partial class AddMediaFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Answer");

            migrationBuilder.AddColumn<string>(
                name: "AudioIds",
                table: "Question",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageIds",
                table: "Question",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageIds",
                table: "Answer",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    FileName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    Extension = table.Column<string>(type: "text", nullable: false),
                    FileByteSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Id",
                table: "MediaFiles",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "AudioIds",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "ImageIds",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "ImageIds",
                table: "Answer");

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "Question",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Question",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Answer",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}
