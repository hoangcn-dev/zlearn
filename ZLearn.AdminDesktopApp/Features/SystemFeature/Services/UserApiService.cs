using System.Net.Http;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Services.APIs;
using ZLearn.Application.Auth.Commands.UpdateUser;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Auth.Queries.GetListUsers;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.AdminDesktopApp.Features.SystemFeature.Services
{
    public interface IUserApiService
    {
        Task<Result<PaginatedDto<UserListItemDto>>> GetListUsers(GetListUsersQuery query);
        Task<Result<UserDetailDto>> GetUserDetailById(string id);
        Task<Result<UpdateResponseDto>> UpdateUser(string id, UserUpdateContentDto data);
        Task<Result<List<string>>> GetListRoles();
    }

    public class UserApiService : BaseApiService, IUserApiService
    {
        public UserApiService(
            IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<Result<List<string>>> GetListRoles()
        {
            var res = await GetAsync<List<string>>("auth/roles");
            return res;
        }

        public async Task<Result<PaginatedDto<UserListItemDto>>> GetListUsers(GetListUsersQuery query)
        {
            var res = await GetAsync<PaginatedDto<UserListItemDto>>("auth/users", query);
            return res;
        }

        public async Task<Result<UserDetailDto>> GetUserDetailById(string id)
        {
            var res = await GetAsync<UserDetailDto>($"auth/users/{id}");
            return res;
        }

        public async Task<Result<UpdateResponseDto>> UpdateUser(string id, UserUpdateContentDto data)
        {
            var res = await PostAsync<UpdateResponseDto>($"auth/users/{id}", data);
            return res;
        }
    }
}
