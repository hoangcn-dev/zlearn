using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ZLearn.AdminDesktopApp.Helpers
{
    public class AppStringHelper
    {
        public static string ToQueryString(object query)
        {
            if (query == null)
                return string.Empty;

            var properties = query.GetType()
                .GetProperties()
                .Where(p => p.GetValue(query) != null)
                .Select(p => $"{p.Name}={Uri.EscapeDataString(p.GetValue(query).ToString())}");

            return string.Join("&", properties);
        }
    }
}
