using ZLearn.Application;
using ZLearn.Infras;

namespace ZLearn.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            builder.Services.AddControllersWithViews();

            services.AddApplicationServices();
            services.AddRouting(opt =>
            {
                opt.LowercaseUrls = true;
            });

            builder.AddRedisService();
            builder.AddIdentityService();
            builder.AddPostgreSQLDataServices();
            builder.AddCloudinaryService();


            var app = builder.Build();
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}");
            app.Run();
        }
    }
}