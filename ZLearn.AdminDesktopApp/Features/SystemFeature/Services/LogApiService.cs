using System.Net.Http;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Logs.DTOs;

namespace ZLearn.AdminDesktopApp.Features.SystemFeature.Services
{
    public interface ILogApiService
    {
        Task<Result<List<LogListItemDto>>> GetLogsOfDay(DateTime? date = null);
    }

    public class LogApiService : BaseApiService, ILogApiService
    {
        public LogApiService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<Result<List<LogListItemDto>>> GetLogsOfDay(DateTime? date = null)
        {
            var res = await GetAsync<List<LogListItemDto>>("logs/history", new { Date = date?.ToString("yyyy-MM-dd") });
            return res;
        }
    }
}
