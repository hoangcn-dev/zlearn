using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ZLearn.Infras.Log
{
    public class LogMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            string userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            using (Serilog.Context.LogContext.PushProperty("UserId", userId))
            using (Serilog.Context.LogContext.PushProperty("IpAddress", ip))
            {
                await next(context);
            }
        }
    }
}
