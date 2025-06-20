using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ZLearn.Application.Mappers;
using ZLearn.Application.Temp.Repositories;
using ZLearn.Infra.Data.Mappers;
using ZLearn.Infra.Data.Repositories;
using ZLearn.Infra.Data.Utils;

namespace ZLearn.Infra.Data.Extensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddPostgreSQL(this IServiceCollection services)
        {
            // Register DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(EnvVariableHelper.GetVariable("POSTGRESQL_CONNECTION_STRING")));

            // Register AutoMapper profiles from this layer
            services.AddAutoMapper(typeof(CommonMapper));
            services.AddAutoMapper(typeof(EntityMapper));

            // Register repositories
            services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<,>));
            //services.AddScoped<IQuizRepo, QuizRepo>();
            services.AddScoped<ICategoryRepo, CategoryRepo>();

            return services;
        }
    }
}