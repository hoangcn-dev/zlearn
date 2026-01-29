using Microsoft.EntityFrameworkCore.Migrations;
using System.Reflection;

#nullable disable

namespace ZLearn.Infras.Migrations
{
    public partial class AddQuartzTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ZLearn.Infras.External.Quartz.up_migration.sql");
            using var reader = new StreamReader(stream!);
            var script = reader.ReadToEnd();

            migrationBuilder.Sql(script);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ZLearn.Infras.External.Quartz.down_migration.sql");
            using var reader = new StreamReader(stream!);
            var script = reader.ReadToEnd();

            migrationBuilder.Sql(script);
        }
    }
}
