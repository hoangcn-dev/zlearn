using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SixLabors.ImageSharp;
using System.Security.Claims;
using System.Threading.Tasks;
using ZLearn.API.Exceptions;
using ZLearn.Application.Auth.Commands.RefreshToken;
using ZLearn.Application.Auth.Commands.SignIn;
using ZLearn.Application.Auth.Commands.SignOut;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.Identity;
using ZLearn.Application.Common.Identity.DTOs;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Infras.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JwtManager _jwtManager;
        private readonly HttpClient _httpClient;
        private readonly IFileRepo _fileRepo;
        private readonly IMediaStoreService _mediaStoreService;

        public IdentityService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            JwtManager jwtManager,
            HttpClient httpClient,
            IFileRepo fileRepo,
            IMediaStoreService mediaStoreService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtManager = jwtManager;
            _httpClient = httpClient;
            _fileRepo = fileRepo;
            _mediaStoreService = mediaStoreService;
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
                ImagePath = string.IsNullOrEmpty(user.ImageId) ?
                    StringHelper.GetDefaultImageUrl() :
                    await _fileRepo.Get(user.ImageId, f => f.SourceUrl) ?? StringHelper.GetDefaultImageUrl(),
                Token = token,
                UserName = data.UserName,
                Roles = roles
            };
        }

        public async Task<UserSessionDataDto> AuthenticateWithGoogle(AuthenticateResult? authenticateResult)
        {
            if(authenticateResult is null || !authenticateResult.Succeeded)
            {
                throw new InvalidCredentialsException("Authentication failed.");
            }

            var claims = authenticateResult.Principal.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? throw new InvalidCredentialsException("Email claim not found.");
            var user = _userManager.Users.FirstOrDefault(u => u.Email == email);
            if (user is null)
            {
                // Create a new user if not found
                var imageUrl = claims.FirstOrDefault(c => c.Type == "image")?.Value;
                user = new AppUser
                {
                    Id = IdGenerator.Generate("ACC"),
                    UserName = StringHelper.GetRandomUserName(),
                    ImageId = string.IsNullOrEmpty(imageUrl)? null : await GetFileMediaFromUrl(imageUrl),
                    Email = email,
                    FirstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "Ẩn danh",
                    LastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "",
                    NickName = StringHelper.GetRandomNickName(),
                    IsShowNickName = true,
                    EmailConfirmed = true
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    throw new InvalidCredentialsException("Failed to create user.");
                var assignRoleResult = await _userManager.AddToRoleAsync(user, nameof(UserRole.User));
                if (!assignRoleResult.Succeeded)
                    throw new DatabaseErrorException("Failed to assign user role.");
            }

            user.LastLogin = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtManager.IssueToken(user, roles, isLogin: true);
            return new UserSessionDataDto
            {
                Id = user.Id,
                ImagePath = string.IsNullOrEmpty(user.ImageId)?
                    StringHelper.GetDefaultImageUrl():
                    await _fileRepo.Get(user.ImageId, f => f.SourceUrl) ?? StringHelper.GetDefaultImageUrl(),
                Token = token,
                UserName = user.UserName,
                Roles = roles
            };
        }

        private async Task<string?> GetFileMediaFromUrl(string url)
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode) return null;
            long? fileSize = response.Content.Headers.ContentLength;
            await using var stream = await response.Content.ReadAsStreamAsync();
            var file = await _mediaStoreService.SaveFile(stream, Path.GetFileName(url), MediaType.Image, CancellationToken.None);
            _fileRepo.Create(file);
            await _fileRepo.SaveChanges();
            return file.Id;
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
