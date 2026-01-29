using System.Net.Http;
using ZLearn.AdminDesktopApp.Services.APIs;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Tracking.DTOs;

namespace ZLearn.AdminDesktopApp.Features.SystemFeature.Services
{
    public interface ISystemApiService
    {
        Task<Result<AccessCountStatDto>> GetAccessTrackingData(DateTime? date = null);
    }

    public class SystemApiService : BaseApiService, ISystemApiService
    {
        public SystemApiService(
            IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<Result<AccessCountStatDto>> GetAccessTrackingData(DateTime? date = null)
        {
            var res = await GetAsync<AccessCountStatDto>("system/access-stat");
            return res;
        }
    }
}
