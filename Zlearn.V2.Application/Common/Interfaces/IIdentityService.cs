using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Identity.DTOs;
using Zlearn.V2.Application.Identity.Commands;
using Zlearn.V2.Application.Identity.Queries;

namespace Zlearn.V2.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<UserSessionDataDto> AuthenticateAsync(SignInCommand data);
        Task<UserSessionDataDto> AuthenticateWithGoogle(AuthenticateResult? authenticateResult);
        Task EndSessionAsync(SignOutCommand data);
        Task<JwtTokenDto> RefreshToken(RefreshTokenCommand data);
        Task<List<string>> GetAllSystemRoles();
        Task<PaginatedDto<UserListItemDto>> GetAllUsers(GetListUsersQuery request);
        Task<UserDetailDto> GetUserDetail(string id);
        Task<UpdateResponseDto> UpdateUser(UpdateUserCommand request);
        Task<UpdateResponseDto> UpdateUserProfile(UpdateUserProfileCommand request);
        Task<Dictionary<string, string>> GetImageUrls(List<string> userIds);
        Task<bool> SetUserStatus(string userId, bool isLockout);
    }
}
