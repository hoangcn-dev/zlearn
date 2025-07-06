using Microsoft.EntityFrameworkCore.Design;

namespace ZLearn.Infras.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionBuilder.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING")!);
            return new AppDbContext(optionBuilder.Options);
        }
    }
}
