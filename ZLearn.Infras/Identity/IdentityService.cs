using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ZLearn.API.Exceptions;
using ZLearn.Application.Auth.Commands.RefreshToken;
using ZLearn.Application.Auth.Commands.SignIn;
using ZLearn.Application.Auth.Commands.SignOut;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.Identity;
using ZLearn.Application.Common.Identity.DTOs;
using ZLearn.Application.Common.Interfaces;

namespace ZLearn.Infras.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JwtManager _jwtManager;

        public IdentityService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            JwtManager jwtManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtManager = jwtManager;
        }

        public async Task<UserSessionDataDto> AuthenticateAsync(SignInCommand data)
        {
            var user = await _userManager.FindByNameAsync(data.UserName)
                ?? throw new NotFoundException("UserName does not exist.");
            var loginResult = await _signInManager.CheckPasswordSignInAsync(user, data.Password, false);
            if (!loginResult.Succeeded)
                throw new InvalidCredentialsException();

            // Create JWT
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtManager.IssueToken(user, roles, isLogin: true);
            return new UserSessionDataDto
            {
                Id = user.Id,
                ImagePath = user.ImagePath,
                Token = token,
                UserName = data.UserName,
                Roles = roles
            };
        }

        public Task<bool> AuthorizeAsync(string userId, string policyName)
        {
            throw new NotImplementedException();
        }

        public Task<IAppUser> CreateAsync(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public Task<IAppUser> DeleteUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task EndSessionAsync(SignOutCommand data)
        {
            await _jwtManager.RevokeToken(data.AccessToken);
        }

        public Task<bool> IsInRoleAsync(string userId, string roleName)
        {
            throw new NotImplementedException();
        }

        public async Task<JwtTokenDto> RefreshToken(RefreshTokenCommand data)
        {
            // Validate access token and refresh token
            var info = _jwtManager.ValidateAccessToken(data.AccessToken)
                ?? throw new TokenExpiredException();
            var user = await _userManager.FindByIdAsync(info.FindFirstValue(ClaimTypes.NameIdentifier))
                ?? throw new TokenExpiredException();
            if (!await _jwtManager.ValidateRefreshToken(data.RefreshToken, user.Id))
                throw new TokenExpiredException();

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtManager.IssueToken(user, roles, isLogin: false);
            return token;
        }

        public Task<bool> SetUserStatus(string userId, bool isLockout)
        {
            throw new NotImplementedException();
        }
    }
}
