using Microsoft.AspNetCore.Identity;
using ZLearn.API.Exceptions;
using ZLearn.Application.Auth.Commands.SignIn;
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

        public Task<bool> IsInRoleAsync(string userId, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetUserStatus(string userId, bool isLockout)
        {
            throw new NotImplementedException();
        }
    }
}
