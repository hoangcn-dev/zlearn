using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ZLearn.API.Exceptions;
using ZLearn.Application.Categories;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Identity;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.Services;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Exams;
using ZLearn.Application.Files;
using ZLearn.Application.LicenseKeys;
using ZLearn.Application.Logs;
using ZLearn.Application.Quizzes;
using ZLearn.Application.Realtime;
using ZLearn.Infras.Data;
using ZLearn.Infras.Data.Interceptors;
using ZLearn.Infras.Data.Repositories;
using ZLearn.Infras.Data.Services;
using ZLearn.Infras.External.AI.Groq;
using ZLearn.Infras.External.CloudinaryStore;
using ZLearn.Infras.External.Redis;
using ZLearn.Infras.External.SignalR;
using ZLearn.Infras.Services;
using ZLearn.Infras.Services.AccessTracking;
using ZLearn.Infras.Services.ExamTracking;
using ZLearn.Infras.Services.Identity;
using ZLearn.Infras.Services.Log;
using ZLearn.Infras.Services.Scheduler;

namespace ZLearn.Infras
{
    public static class DependencyInjection
    {
        public static void AddPostgreSQLDataServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IAppDbContext, AppDbContext>();
            builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            builder.Services.AddScoped<ISaveChangesInterceptor, DispatchEventsInterceptor>();
            builder.Services.AddScoped(typeof(IBaseRepo<>), typeof(BaseRepo<>));
            builder.Services.AddScoped<ICateRepo, CateRepo>();
            builder.Services.AddScoped<IFileRepo, FileRepo>();
            builder.Services.AddScoped<IQuizRepo, QuizRepo>();
            builder.Services.AddScoped<IQuestionRepo, QuestionRepo>();
            builder.Services.AddScoped<IAccessHistoryRepo, AccessHistoryRepo>();
            builder.Services.AddScoped<ILicenseKeyRepo, LicenseKeyRepo>();
            builder.Services.AddScoped<IExamRepo, ExamRepo>();
            builder.Services.AddScoped<IDocumentExportService, DocumentExportService>();
            builder.Services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(EnvVariableHelper.GetValue(EnvVariableNames.CONNECTION_STRING_POSTGRES));
            });
        }

        public static void AddGroqServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IAIService, GroqService>();
        }

        public static void AddQuartzServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddQuartz(q =>
            {
                 q.UsePersistentStore(store =>
                 {
                     store.UsePostgres(EnvVariableHelper.GetValue(EnvVariableNames.CONNECTION_STRING_POSTGRES));
                     store.UseNewtonsoftJsonSerializer();
                 });
            });
            builder.Services.AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
            });
        }
        
        public static void AddRedisService(this WebApplicationBuilder builder) 
        {
            builder.Services.Configure<RedisConfig>(builder.Configuration.GetSection("Redis"));
            builder.Services.AddSingleton<IRedisService, RedisService>();
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = EnvVariableHelper.GetValue(EnvVariableNames.CONNECTION_STRING_REDIS);
                var redisConfig = ConfigurationOptions.Parse(connectionString);
                var configs = builder.Configuration.GetSection("Redis").Get<RedisConfig>();
                if (configs != null)
                {
                    redisConfig.Ssl = configs.Ssl;
                    redisConfig.ConnectTimeout = configs.ConnectTimeout;
                    redisConfig.SyncTimeout = configs.SyncTimeout;
                    redisConfig.ConnectRetry = configs.ConnectRetry;
                }
                return ConnectionMultiplexer.Connect(redisConfig);
            });
        }

        public static void AddCloudinaryService(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IMediaStoreService, CloudinaryService>();
        }

        public static void AddIdentityService(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<Initializer>();
            builder.Services.AddIdentity<AppUser, AppRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<JwtManager>();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JWT"));
            builder.Services
                .AddAuthentication(opt =>
                {
                    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        ValidAudience = builder.Configuration["JWT:Audience"],
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(EnvVariableHelper.GetValue(EnvVariableNames.JWT_SECRET_KEY))),
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.Name
                    };

                    opt.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = async context =>
                        {
                            var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                context.Response.ContentType = "application/json";
                                var result = JsonSerializer.Serialize(Result<NoData>.Failure("", ErrorCodes.TOKEN_EXPIRED), jsonOptions);
                                await context.Response.WriteAsync(result);
                                return;
                            }
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(JsonSerializer.Serialize(Result<NoData>.Failure("", ErrorCodes.UNAUTHORIZED), jsonOptions));
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
                    opt.ClientId = EnvVariableHelper.GetValue(EnvVariableNames.GOOGLE_CLIENT_ID);
                    opt.ClientSecret = EnvVariableHelper.GetValue(EnvVariableNames.GOOGLE_CLIENT_SECRET);
                    opt.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    opt.ClaimActions.MapJsonKey("image", "picture");
                    opt.CallbackPath = builder.Configuration["GoogleAuth:CallbackPath"];
                });

            builder.Services.AddAuthorization(opt =>
            {
                opt.AddPolicy("OnlyAdmin", policy =>
                {
                    policy.RequireClaim(ClaimTypes.Role, nameof(UserRole.Admin));
                });
            });
        }

        public static async Task InitializeDatabase(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<Initializer>();
            await initializer.InitializeDatabaseAsync();
            await initializer.InitializeDataAsync();
        }

        public static void AddDatabaseBackupService(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<DatabasebackupConfiguration>(builder.Configuration.GetSection("Backup"));
            builder.Services.AddHostedService<DatabaseBackupService>();
        }

        public static void AddFileCleanupService(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<FileCleanupConfiguration>(builder.Configuration.GetSection("FileCleanup"));
            builder.Services.AddHostedService<RemoveUnusedFilesService>();
        }

        public static void AddRealtimeServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddSignalR();
        }

        public static void AddSchedulerService(this WebApplicationBuilder builder)
        {
            builder.Services.AddHangfire(config =>
            {
                config.UseMemoryStorage();
            });
            builder.Services.AddHangfireServer();
            builder.Services.AddSingleton<ISchedulerService, SchedulerService>();
        }

        #region Access Tracking
        public static void AddAccessTrackingService(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IAccessTrackingService, AccessTrackingService>();
            builder.Services.Configure<AccessTrackingConfig>(
                builder.Configuration.GetSection("AccessTrackingConfig"));
            builder.Services.AddHostedService<AutoSaveAccessCountService>();
        }
        public static void UseAccessTracking(this WebApplication app)
        {
            app.MapHub<AccessTrackingHub>("/access-tracking");
        }
        #endregion

        #region Exam Tracking
        public static void AddExamTrackingService(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<ConcurrentDictionary<string, string>>();
            builder.Services.AddSingleton<IExamTrackingService, ExamTrackingService>();
            //builder.Services.AddHostedService<AutoSaveAccessCountService>();
        }
        public static void UseExamTracking(this WebApplication app)
        {
            app.MapHub<ExamHub>(ExamHub.HUB_URL);
        }
        #endregion

        #region Log
        public static void AddLogService(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<ILogService, LogService>();
            builder.Services.AddTransient<LogMiddleware>();
            builder.Host.UseSerilog((services, config) =>
            {
                config
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // Loại bỏ log Information từ Microsoft
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                    .WriteTo.File(
                        path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, builder.Configuration["Serilog:WriteTo:0:Args:path"]),
                        rollingInterval: (RollingInterval)Enum.Parse(typeof(RollingInterval), builder.Configuration["Serilog:WriteTo:0:Args:rollingInterval"], true),
                        outputTemplate: builder.Configuration["Serilog:WriteTo:0:Args:outputTemplate"]
                    )
                    .WriteTo.Console(
                        outputTemplate: builder.Configuration["Serilog:WriteTo:1:Args:outputTemplate"]
                    );
            });
        }

        public static void UseLogMiddleware(this WebApplication app)
        {
            app.UseMiddleware<LogMiddleware>();
        } 
        #endregion
    }
}
