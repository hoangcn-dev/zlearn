using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SixLabors.ImageSharp;
using System.Security.Claims;
using System.Threading.Tasks;
using ZLearn.API.Exceptions;
using ZLearn.Application.Auth.Commands.RefreshToken;
using ZLearn.Application.Auth.Commands.SignIn;
using ZLearn.Application.Auth.Commands.SignOut;
using ZLearn.Application.Auth.Commands.UpdateUser;
using ZLearn.Application.Auth.Commands.UpdateUserProfile;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Auth.Queries.GetListUsers;
using ZLearn.Application.Common.DTOs;
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
        private readonly RoleManager<AppRole> _roleManager;
        private readonly JwtManager _jwtManager;
        private readonly HttpClient _httpClient;
        private readonly IFileRepo _fileRepo;
        private readonly IMediaStoreService _mediaStoreService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            JwtManager jwtManager,
            HttpClient httpClient,
            IFileRepo fileRepo,
            IMediaStoreService mediaStoreService,
            RoleManager<AppRole> roleManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtManager = jwtManager;
            _httpClient = httpClient;
            _fileRepo = fileRepo;
            _mediaStoreService = mediaStoreService;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
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
                ImagePath = user.ImageUrl ?? StringHelper.GetDefaultImageUrl(),
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
                    ImageUrl = string.IsNullOrEmpty(imageUrl)? null : await GetFileMediaFromUrl(imageUrl),
                    Email = email,
                    FirstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "Ẩn danh",
                    LastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "",
                    NickName = StringHelper.GetRandomNickName(),
                    IsShowNickName = true,
                    EmailConfirmed = true,
                    IsActive = true,
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    throw new InvalidCredentialsException("Failed to create user.");
                var assignRoleResult = await _userManager.AddToRoleAsync(user, nameof(UserRole.User));
                if (!assignRoleResult.Succeeded)
                    throw new DatabaseErrorException("Failed to assign user role.");
            }

            if (user.IsActive == false)
            {
                throw new ForbiddenException("User account is locked.");
            }

            user.LastLogin = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtManager.IssueToken(user, roles, isLogin: true);
            return new UserSessionDataDto
            {
                Id = user.Id,
                ImagePath = user.ImageUrl ?? StringHelper.GetDefaultImageUrl(),
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
            return file.SourceUrl;
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

        public async Task<List<string>> GetAllSystemRoles()
        {
            return await _roleManager.Roles
                .Select(r => r.Name)
                .ToListAsync();
        }

        public async Task<PaginatedDto<UserListItemDto>> GetAllUsers(GetListUsersQuery request)
        {
            var filterBuilder = new FilterBuilder<AppUser>();
            if (!string.IsNullOrEmpty(request.Email))
            {
                filterBuilder.AndCondition(u => u.Email.Contains(request.Email));
            }

            var query = _userManager.Users.AsNoTracking();
            query = query.Where(filterBuilder.GetPredicateOrDefault());

            var totalItems = await query.CountAsync();
            var pagedUsers = await query
                .OrderBy(u => u.UserName)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var items = new List<UserListItemDto>();
            foreach (var user in pagedUsers)
            {
                var roles = (List<string>)await _userManager.GetRolesAsync(user);
                items.Add(new UserListItemDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    NickName = user.NickName,
                    EmailConfirmed = user.EmailConfirmed,
                    IsActive = user.IsActive,
                    LastLogin = user.LastLogin,
                    Roles = roles
                });
            }

            return new PaginatedDto<UserListItemDto>
            {
                TotalItems = totalItems,
                Items = items,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };
        }

        public async Task<UserDetailDto> GetUserDetail(string id)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new NotFoundException("User", id);
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDetailDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                NickName = user.NickName,
                PhoneNumber = user.PhoneNumber,
                ImagePath = user.ImageUrl ?? StringHelper.GetDefaultImageUrl(),
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                LastLogin = user.LastLogin,
                Roles = roles.ToList(),
                IsShowNickName = user.IsShowNickName,
            };
        }

        public async Task<UpdateResponseDto> UpdateUser(UpdateUserConmand request)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id)
                ?? throw new NotFoundException("User", request.Id);

            var data = request.UpdateData;
            if (data.UserName != user.UserName && await _userManager.Users.AnyAsync(u => u.UserName == data.UserName))
                throw new ResourceConflictException("UserName already exists.");
            if (!string.IsNullOrEmpty(data.ImageUrl))
            {
                if (user.ImageUrl != null)
                    await _fileRepo.DeleteFileByUrls(new List<string> { user.ImageUrl });
                user.ImageUrl = data.ImageUrl;
                await _fileRepo.SetUsing(new List<string> { data.ImageUrl });
            }

            user.UserName = data.UserName;
            user.IsActive = data.IsActive;
            user.FirstName = data.FirstName;
            user.LastName = data.LastName;
            user.NickName = data.NickName;
            user.IsActive = data.IsActive;
            user.EmailConfirmed = data.EmailConfirmed;
            user.PhoneNumber = data.PhoneNumber;

            // Update user roles
            var oldRoles = (await _userManager.GetRolesAsync(user)).ToHashSet();
            foreach (var newRole in data.Roles)
            {
                if (oldRoles.Contains(newRole))
                {
                    oldRoles.Remove(newRole);
                }
                else
                {
                    var assignRoleResult = await _userManager.AddToRoleAsync(user, newRole);
                    if (!assignRoleResult.Succeeded)
                        throw new DatabaseErrorException($"Failed to assign role {newRole} to user {user.UserName}.");
                }
            }
            await _userManager.RemoveFromRolesAsync(user, oldRoles);
            await _userManager.UpdateAsync(user);
            await _fileRepo.SaveChanges();

            return new UpdateResponseDto
            {
                Id = user.Id,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        public async Task<UpdateResponseDto> UpdateUserProfile(UpdateUserProfileCommand request)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id)
                ?? throw new NotFoundException("User", request.Id);

            var data = request.UpdateData;

            if (!string.IsNullOrEmpty(data.ImageUrl))
            {
                if (user.ImageUrl != null)
                    await _fileRepo.DeleteFileByUrls(new List<string> { user.ImageUrl });
                user.ImageUrl = data.ImageUrl;
                await _fileRepo.SetUsing(new List<string> { data.ImageUrl });
            }

            user.FirstName = data.FirstName;
            user.LastName = data.LastName;
            user.NickName = data.NickName;
            user.PhoneNumber = data.PhoneNumber;
            user.IsShowNickName = data.IsShowNickName;

            await _userManager.UpdateAsync(user);
            await _fileRepo.SaveChanges();

            return new UpdateResponseDto
            {
                Id = user.Id,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
