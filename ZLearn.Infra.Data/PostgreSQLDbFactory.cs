using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ZLearn.Infra.Data.Utils;

namespace ZLearn.Infra.Data
{
    public class PostgreSQLDbFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionBuilder.UseNpgsql(EnvVariableHelper.GetVariable("POSTGRESQL_CONNECTION_STRING"));

            return new AppDbContext(optionBuilder.Options);
        }
    }
}
