using System;
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
using ZLearn.Application.Common.Utils;
using V2Utils = Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Domain.CatalogContext.Categories.Events;
using Zlearn.V2.Application.Quizzes;

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

            // Cấu hình AutoMapper cho cả V1 và V2 (ghi đè cấu hình V1 trước đó)
            services.AddAutoMapper(
                typeof(ZLearn.Application.DependencyInjection).Assembly,
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

            // 4. Đăng ký background worker xử lý Outbox V2
            services.AddHostedService<OutboxProcessorJob>();

            // 5. Đăng ký Projection Handler cho V2
            services.AddTransient<INotificationHandler<OutboxEvent>, SyncCategoryToMongoHandler>();
            services.AddTransient<INotificationHandler<OutboxEvent>, SyncQuizToMongoHandler>();

            return services;
        }
    }
}
