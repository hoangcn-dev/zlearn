using ZLearn.Application.Categories;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.Utils;
using ZLearn.Infras.Data;
using ZLearn.Infras.Data.Interceptors;
using ZLearn.Infras.Data.Repositories;

namespace ZLearn.Infras
{
    public static class DependencyInjection
    {
        public static void AddPostgreSQLDataServices(this IServiceCollection services)
        {
            services.AddScoped<IAppDbContext, AppDbContext>();
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchEventsInterceptor>();
            services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));
            services.AddScoped<ICateRepo, CateRepo>();
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(EnvVariableHelper.GetValue(EnvVariableHelper.Names.POSTGRESQL_CONNECTION_STRING));
            });
        }
    }
}
