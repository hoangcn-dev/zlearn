using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using ZLearn.API.Middlewares;
using ZLearn.Application;
using ZLearn.Infras;

namespace ZLearn.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var services = builder.Services;

            services.AddExceptionMiddleware();
            services.AddApplicationServices();
            services.AddPostgreSQLDataServices();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ZLearn API",
                    Version = "v1",
                    Description = "API for ZLearn Quiz Application",
                    Contact = new OpenApiContact
                    {
                        Name = "ZLearn Team",
                        Email = "support@zlearn.com"
                    }
                });
            });

            services.AddRouting(opt =>
            {
                opt.LowercaseUrls = true;
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionMiddleware();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
