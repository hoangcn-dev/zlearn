using Microsoft.EntityFrameworkCore.Design;

namespace ZLearn.Infras.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionBuilder.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING_POSTGRES")!);
            return new AppDbContext(optionBuilder.Options);
        }
    }
}
