using ZLearn.Application.Auth.Commands.RefreshToken;
using ZLearn.Application.Auth.Commands.SignIn;
using ZLearn.Application.Auth.Commands.SignOut;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.Identity.DTOs;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Common.Identity
{
    public interface IIdentityService
    {
        Task<bool> IsInRoleAsync(string userId, string roleName);
        Task<bool> AuthorizeAsync(string userId, string policyName);
        Task<UserSessionDataDto> AuthenticateAsync(SignInCommand data);
        Task EndSessionAsync(SignOutCommand data);
        Task<JwtTokenDto> RefreshToken(RefreshTokenCommand data);
        Task<IAppUser> CreateAsync(string userName, string password);
        Task<IAppUser> DeleteUserAsync(string userId);
        Task<bool> SetUserStatus(string userId, bool isLockout);
    }
}
