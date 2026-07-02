using System;
using System.Linq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Categories.DTOs;
using Zlearn.V2.Infas.Data;
using Zlearn.V2.Infas.Data.Interceptors;
using Zlearn.V2.Infas.Data.Outbox;
using Zlearn.V2.Infas.Data.Repositories;
using Zlearn.V2.Infas.Data.Services;
using Zlearn.V2.Infas.Services.Projections;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Infas.External.CloudinaryStore;
using V2Utils = Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Domain.CatalogContext.Categories.Events;
using Zlearn.V2.Application.Quizzes;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Application.Identity;
using StackExchange.Redis;
using Quartz;
using Hangfire;
using Hangfire.MemoryStorage;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Zlearn.V2.Infas.Identity;
using Zlearn.V2.Infas.Identity.Services;
using Zlearn.V2.Application.Identity.DTOs;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.DTOs;

namespace Zlearn.V2.Infas
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddV2Services(this IServiceCollection services)
        {
            // 1. Đăng ký AppDbContext cho V2 (dùng chung Connection String PostgreSQL)
            services.AddScoped<HandleEventsInterceptor>();
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.AddInterceptors(
                    sp.GetRequiredService<HandleEventsInterceptor>(),
                    sp.GetRequiredService<AuditableEntityInterceptor>()
                );
                options.UseNpgsql(V2Utils.EnvVariableHelper.GetValue(V2Utils.EnvVariableNames.CONNECTION_STRING_POSTGRES));
            });

            // Đăng ký MediatR cho các handlers của V2 Application
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(Zlearn.V2.Application.Categories.Commands.CreateCategory.CreateCategoryCommand).Assembly);
            });

            // Cấu hình AutoMapper cho V2
            services.AddAutoMapper(
                typeof(Zlearn.V2.Application.Categories.Commands.CreateCategory.CreateCategoryCommand).Assembly
            );

            // 2. Đăng ký MongoDB cho V2 (nếu chưa được đăng ký trước đó ở lõi chính)
            var mongoConnString = V2Utils.EnvVariableHelper.GetValue(V2Utils.EnvVariableNames.CONNECTION_STRING_MONGODB);
            var mongoDatabaseName = V2Utils.EnvVariableHelper.GetValue(V2Utils.EnvVariableNames.MONGODB_DATABASE_NAME);

            // Đăng ký MongoClient dạng Singleton
            services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnString));
            services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoDatabaseName);
            });

            // 3. Đăng ký generic repositories cho V2
            services.AddScoped(typeof(IWriteRepo<>), typeof(WriteRepo<>));
            services.AddScoped(typeof(IReadRepo<>), typeof(ReadRepo<>));
            services.AddScoped<IQuizWriteRepo, QuizWriteRepo>();

            // Đăng ký File repository và Media store service V2
            services.AddScoped<IFileRepo, FileRepo>();
            services.AddScoped<IMediaStoreService, CloudinaryService>();
            services.AddScoped<IExamRepo, ExamRepo>();
            services.AddScoped<IAppUserRepo, AppUserRepo>();
            services.AddSingleton<Zlearn.V2.Application.Common.Interfaces.IRedisService, RedisService>();
            services.AddScoped<Zlearn.V2.Application.Common.Interfaces.ISchedulerService, SchedulerService>();
            services.AddScoped<Zlearn.V2.Application.Common.Interfaces.IIdentityService, IdentityService>();
            services.AddScoped<Zlearn.V2.Application.Exams.IExamTrackingService, ExamTrackingService>();
            services.AddScoped<Zlearn.V2.Application.Common.Interfaces.IExamSessionService, ExamSessionService>();
            services.AddTransient<LogMiddleware>();
            services.AddScoped<Initializer>();
            services.AddHttpClient();

            // 4. Đăng ký background worker xử lý Outbox V2 và thay thế Hosted Service chấm điểm V1 bằng V2
            var v1GradingServiceDescriptor = services.FirstOrDefault(d => d.ImplementationType?.FullName == "ZLearn.Infras.Data.Services.ExamGradingBackgroundService");
            if (v1GradingServiceDescriptor != null)
            {
                services.Remove(v1GradingServiceDescriptor);
            }
            services.AddHostedService<Zlearn.V2.Infas.Data.Services.ExamGradingBackgroundService>();
            services.AddHostedService<OutboxProcessorJob>();

            // 5. Đăng ký Projection Handler cho V2
            services.AddTransient<INotificationHandler<OutboxEvent>, SyncCategoryToMongoHandler>();
            services.AddTransient<INotificationHandler<OutboxEvent>, SyncQuizToMongoHandler>();
            services.AddTransient<INotificationHandler<OutboxEvent>, SyncExamToMongoHandler>();

            return services;
        }

        public static IServiceCollection AddV2IdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<AppIdentityUser, AppIdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddHttpContextAccessor();
            services.AddSingleton<JwtManager>();
            services.Configure<JwtConfig>(configuration.GetSection("JWT"));

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(V2Utils.EnvVariableHelper.GetValue(V2Utils.EnvVariableNames.JWT_SECRET_KEY))),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };

                opt.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async context =>
                    {
                        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            var result = System.Text.Json.JsonSerializer.Serialize(Result<NoData>.Failure("", ErrorCodes.TOKEN_EXPIRED), jsonOptions);
                            await context.Response.WriteAsync(result);
                            return;
                        }
                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(Result<NoData>.Failure("", ErrorCodes.UNAUTHORIZED), jsonOptions));
                    },
                    OnChallenge = context =>
                    {
                        throw new UnauthorizedException();
                    }
                };
            })
            .AddCookie(opt =>
            {
                opt.Cookie.SameSite = SameSiteMode.Lax;
            })
            .AddGoogle("Google", opt =>
            {
                opt.ClientId = V2Utils.EnvVariableHelper.GetValue(V2Utils.EnvVariableNames.GOOGLE_CLIENT_ID);
                opt.ClientSecret = V2Utils.EnvVariableHelper.GetValue(V2Utils.EnvVariableNames.GOOGLE_CLIENT_SECRET);
                opt.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                opt.ClaimActions.MapJsonKey("image", "picture");
                opt.CallbackPath = configuration["GoogleAuth:CallbackPath"];
            });

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("OnlyAdmin", policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, nameof(UserRole.Admin));
                });
            });

            return services;
        }

        public static IServiceCollection AddV2RedisService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisConfig>(configuration.GetSection("Redis"));
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = V2Utils.EnvVariableHelper.GetValue(V2Utils.EnvVariableNames.CONNECTION_STRING_REDIS);
                var redisConfig = ConfigurationOptions.Parse(connectionString);
                var configs = configuration.GetSection("Redis").Get<RedisConfig>();
                if (configs != null)
                {
                    redisConfig.Ssl = configs.Ssl;
                    redisConfig.ConnectTimeout = configs.ConnectTimeout;
                    redisConfig.SyncTimeout = configs.SyncTimeout;
                    redisConfig.ConnectRetry = configs.ConnectRetry;
                }
                return ConnectionMultiplexer.Connect(redisConfig);
            });
            return services;
        }

        public static IServiceCollection AddV2QuartzServices(this IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                q.UsePersistentStore(store =>
                {
                    store.UsePostgres(V2Utils.EnvVariableHelper.GetValue(V2Utils.EnvVariableNames.CONNECTION_STRING_POSTGRES));
                    store.UseNewtonsoftJsonSerializer();
                });
            });
            services.AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
            });
            return services;
        }

        public static IServiceCollection AddV2RealtimeServices(this IServiceCollection services)
        {
            var redisConnString = V2Utils.EnvVariableHelper.GetValue(V2Utils.EnvVariableNames.CONNECTION_STRING_REDIS);
            services.AddSignalR().AddStackExchangeRedis(redisConnString);
            return services;
        }

        public static IServiceCollection AddV2SchedulerService(this IServiceCollection services)
        {
            services.AddHangfire(config =>
            {
                config.UseMemoryStorage();
            });
            services.AddHangfireServer();
            return services;
        }

        public static IServiceCollection AddV2DatabaseBackupService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DatabasebackupConfiguration>(configuration.GetSection("Backup"));
            services.AddHostedService<DatabaseBackupService>();
            return services;
        }

        public static IServiceCollection AddV2FileCleanupService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FileCleanupConfiguration>(configuration.GetSection("FileCleanup"));
            services.AddHostedService<RemoveUnusedFilesService>();
            return services;
        }

        public static void AddV2LogService(this ConfigureHostBuilder host, IConfiguration configuration)
        {
            host.UseSerilog((services, config) =>
            {
                config
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration["Serilog:WriteTo:0:Args:path"]!),
                        rollingInterval: (RollingInterval)Enum.Parse(typeof(RollingInterval), configuration["Serilog:WriteTo:0:Args:rollingInterval"]!, true),
                        outputTemplate: configuration["Serilog:WriteTo:0:Args:outputTemplate"]!
                    )
                    .WriteTo.Console(
                        outputTemplate: configuration["Serilog:WriteTo:1:Args:outputTemplate"]!
                    );
            });
        }

        public static void UseV2LogMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<LogMiddleware>();
        }

        public static async Task InitializeV2Database(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<Initializer>();
            await initializer.InitializeDatabaseAsync();
            await initializer.InitializeDataAsync();
        }

        public class RedisConfig
        {
            public bool Ssl { get; set; }
            public int ConnectTimeout { get; set; }
            public int SyncTimeout { get; set; }
            public int ConnectRetry { get; set; }
        }
    }
}
