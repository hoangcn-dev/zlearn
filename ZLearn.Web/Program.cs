using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using ZLearn.Application;
using ZLearn.Domain.Exceptions;
using ZLearn.Infras;
using Zlearn.V2.Infas;
using ZLearn.Web.Middlewares;

namespace ZLearn.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
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
            builder.AddQuartzServices();
            builder.AddGroqServices();
            builder.AddExamTrackingService();
            builder.AddIdentityService();
            builder.AddPostgreSQLDataServices();
            builder.AddMongoDBDataServices();
            builder.Services.AddV2Services();
            builder.AddCloudinaryService();
            builder.AddDatabaseBackupService();
            builder.AddFileCleanupService();
            builder.AddRealtimeServices(); // Keep this for ExamHub
            builder.AddLogService();
            builder.AddSchedulerService();

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

            // Override validation error message
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var firstError = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "Invalid request data.";
                    throw new ValidationErrorException(firstError);
                };
            });

            var app = builder.Build();
            app.UseExceptionMiddleware();
            app.UseJwtMiddleware();
            app.UseForwardedHeaders();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseLogMiddleware();
            app.UseExamTracking();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}");
            app.MapControllers();
            await app.InitializeDatabase();
            app.Run();
        }
    }
}