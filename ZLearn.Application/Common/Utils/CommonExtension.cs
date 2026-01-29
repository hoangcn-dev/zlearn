using Microsoft.AspNetCore.Http;

namespace ZLearn.Application.Common.Utils
{
    public static class CommonExtension
    {
        public static string? GetIPAddress(this HttpContext context)
        {
            string? ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                        context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip)) 
                return context.Connection.RemoteIpAddress?.ToString();
            return ip.Split(',')[0].Trim();
        }
    }
}
