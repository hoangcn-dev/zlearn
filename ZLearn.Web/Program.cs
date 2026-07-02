using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Zlearn.V2.Infas;
using Zlearn.V2.Infas.Data;
using Zlearn.V2.Application.Common.Exceptions;
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

            builder.Services.AddV2RedisService(builder.Configuration);
            builder.Services.AddV2QuartzServices();
            builder.Services.AddV2IdentityServices(builder.Configuration);
            builder.Services.AddV2Services();
            builder.Services.AddV2DatabaseBackupService(builder.Configuration);
            builder.Services.AddV2FileCleanupService(builder.Configuration);
            builder.Services.AddV2RealtimeServices();
            builder.Services.AddV2SchedulerService();
            builder.Host.AddV2LogService(builder.Configuration);

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
            app.UseV2LogMiddleware();
            
            // Map SignalR Hub
            app.MapHub<Zlearn.V2.Infas.External.SignalR.ExamHub>(Zlearn.V2.Infas.External.SignalR.ExamHub.HUB_URL);
            
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}");
            app.MapControllers();
            
            await app.InitializeV2Database();
            
            app.Run();
        }
    }
}