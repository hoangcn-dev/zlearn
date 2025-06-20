using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Application.Common.Utils;

namespace ZLearn.Infras.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionBuilder.UseNpgsql(EnvVariableHelper.GetValue(EnvVariableHelper.Names.POSTGRESQL_CONNECTION_STRING));
            return new AppDbContext(optionBuilder.Options);
        }
    }
}
