using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using ZLearn.Application.Categories;
using ZLearn.Application.Common.Identity;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.Services;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;
using ZLearn.Application.Quizzes;
using ZLearn.Infras.Data;
using ZLearn.Infras.Data.Interceptors;
using ZLearn.Infras.Data.Repositories;
using ZLearn.Infras.External.CloudinaryStore;
using ZLearn.Infras.External.Redis;
using ZLearn.Infras.Identity;

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
            builder.Services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(EnvVariableHelper.GetValue(EnvVariableNames.POSTGRESQL_CONNECTION_STRING));
            });
        }

        public static void AddRedisService(this WebApplicationBuilder builder) 
        {
            builder.Services.Configure<RedisConfig>(builder.Configuration.GetSection("Redis"));
            builder.Services.AddSingleton<IRedisService, RedisService>();
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configs = builder.Configuration.GetSection("Redis").Get<RedisConfig>();
                var redisConfig = new ConfigurationOptions
                {
                    EndPoints = { configs.EndPoints.Default },
                    Password = EnvVariableHelper.GetValue(EnvVariableNames.REDIS_CONNECTION_PASSWORD), // Mật khẩu đã thiết lập
                    Ssl = configs.Ssl,
                    ConnectTimeout = configs.ConnectTimeout,
                    SyncTimeout = configs.SyncTimeout,
                    ConnectRetry = configs.ConnectRetry
                };
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
            builder.Services.AddSingleton<JwtManager>();
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
                            Encoding.UTF8.GetBytes(EnvVariableHelper.GetValue(EnvVariableNames.JWT_SECRET_KEY)))
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

        public static async void InitializeDatabase(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<Initializer>();
            await initializer.InitializeDatabaseAsync();
            await initializer.InitializeDataAsync();
        }
    }
}
