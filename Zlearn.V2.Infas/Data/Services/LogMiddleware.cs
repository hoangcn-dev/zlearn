using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Zlearn.V2.Infas.Data.Services
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
