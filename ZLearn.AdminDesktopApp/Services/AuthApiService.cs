using System.Net.Http;
using ZLearn.Application.Auth.Commands.SignIn;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.AdminDesktopApp.Services
{
    public interface IAuthApiService
    {
        Task<Result<UserSessionDataDto>> SignInAsync(SignInRequestDto data);
    }

    public class AuthApiService : BaseApiService, IAuthApiService
    {
        public AuthApiService(
            IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<Result<UserSessionDataDto>> SignInAsync(SignInRequestDto data)
        {
            var res = await PostAsync<UserSessionDataDto>("auth/sign-in", data);
            return res;
        }
    }
}
