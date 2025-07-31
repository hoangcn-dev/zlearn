using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;
using ZLearn.Application;
using ZLearn.Infras;
using ZLearn.Web.Middlewares;

namespace ZLearn.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;

            services.AddExceptionMiddleware();
            services.AddJwtMiddleware();
            services.AddApplicationServices();
            services.AddRouting(opt => opt.LowercaseUrls = true);
            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocal",
                    builder => builder
                        .WithOrigins("https://localhost:7284")
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            builder.AddRedisService();
            builder.AddIdentityService();
            builder.AddPostgreSQLDataServices();
            builder.AddCloudinaryService();
            builder.AddDatabaseBackupService();
            builder.AddAccessTrackingService();
            builder.AddRealtimeServices();
            builder.AddLogService();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });
            builder.Services.AddControllersWithViews();

            // Fix bug redirect_uri (gg auth) not correct, that cause by nginx redirect from https => http
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });


            var app = builder.Build();
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseForwardedHeaders();
            app.UseExceptionMiddleware();
            app.UseJwtMiddleware();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseLogMiddleware();
            app.UseAccessTracking();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}");
            app.MapControllers();
            app.InitializeDatabase();
            app.Run();
        }
    }
}