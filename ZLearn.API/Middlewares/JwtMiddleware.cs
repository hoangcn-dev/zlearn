
using ZLearn.Infras.Identity;

namespace ZLearn.API.Middlewares
{
    public class JwtMiddleware : IMiddleware
    {
        private readonly JwtManager _jwtService;

        public JwtMiddleware(JwtManager jwtService)
        {
            _jwtService = jwtService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var token = context.Request.Cookies["token"];
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Cookie: " + token);
                context.Request.Headers.Authorization = "Bearer " + token;
            }
            Console.WriteLine("Authorization: " + token);
            await next(context);
        }
    }

    public static class JwtMiddlewareExtensions
    {
        public static void AddJwtMiddleware(this IServiceCollection services)
        {
            services.AddTransient<JwtMiddleware>();
        }

        public static void UseJwtMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<JwtMiddleware>();
        }
    }
}
