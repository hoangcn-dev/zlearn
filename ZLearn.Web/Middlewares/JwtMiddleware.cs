using ZLearn.API.Exceptions;
using ZLearn.Infras.Services.Identity;

namespace ZLearn.Web.Middlewares
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
                if (await _jwtService.IsRevokedToken(token))
                {
                    context.Response.Cookies.Delete("token");
                    throw new UnauthorizedException("Access token has been revoked.");
                }
                context.Request.Headers.Authorization = "Bearer " + token;
            }
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
