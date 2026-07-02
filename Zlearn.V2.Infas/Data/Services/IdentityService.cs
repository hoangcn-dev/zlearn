using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Application.Identity.Commands;
using Zlearn.V2.Application.Identity.DTOs;
using Zlearn.V2.Application.Identity.Queries;
using Zlearn.V2.Domain.FileContext.MediaFiles;
using Zlearn.V2.Infas.Identity;
using Zlearn.V2.Infas.Identity.Services;

namespace Zlearn.V2.Infas.Data.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly RoleManager<AppIdentityRole> _roleManager;
        private readonly JwtManager _jwtManager;
        private readonly HttpClient _httpClient;
        private readonly IFileRepo _fileRepo;
        private readonly IMediaStoreService _mediaStoreService;

        public IdentityService(
            UserManager<AppIdentityUser> userManager,
            SignInManager<AppIdentityUser> signInManager,
            RoleManager<AppIdentityRole> roleManager,
            JwtManager jwtManager,
            HttpClient httpClient,
            IFileRepo fileRepo,
            IMediaStoreService mediaStoreService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtManager = jwtManager;
            _httpClient = httpClient;
            _fileRepo = fileRepo;
            _mediaStoreService = mediaStoreService;
        }

        public async Task<UserSessionDataDto> AuthenticateAsync(SignInCommand data)
        {
            var user = await _userManager.FindByNameAsync(data.UserName)
                ?? throw new NotFoundException("UserName does not exist.");

            if (!user.IsActive)
            {
                throw new ForbiddenException("User account is locked.");
            }

            var loginResult = await _signInManager.CheckPasswordSignInAsync(user, data.Password, false);
            if (!loginResult.Succeeded)
                throw new InvalidCredentialsException();

            user.LastLogin = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtManager.IssueToken(user, roles, isLogin: true);

            return new UserSessionDataDto
            {
                Id = user.Id,
                ImagePath = user.ImageUrl ?? StringHelper.GetDefaultImageUrl(),
                Token = token,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Roles = roles.ToList()
            };
        }

        public async Task<UserSessionDataDto> AuthenticateWithGoogle(AuthenticateResult? authenticateResult)
        {
            if (authenticateResult is null || !authenticateResult.Succeeded)
            {
                throw new InvalidCredentialsException("Authentication failed.");
            }

            var claims = authenticateResult.Principal.Claims;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                ?? throw new InvalidCredentialsException("Email claim not found.");
            
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                var imageUrl = claims.FirstOrDefault(c => c.Type == "image")?.Value;
                user = new AppIdentityUser
                {
                    Id = IdGenerator.Generate("ACC"),
                    UserName = StringHelper.GetRandomUserName(),
                    ImageUrl = string.IsNullOrEmpty(imageUrl) ? null : await GetFileMediaFromUrl(imageUrl),
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

            if (!user.IsActive)
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
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Roles = roles.ToList()
            };
        }

        private async Task<string?> GetFileMediaFromUrl(string url)
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode) return null;
            await using var stream = await response.Content.ReadAsStreamAsync();
            var file = await _mediaStoreService.SaveFile(stream, Path.GetFileName(url), MediaType.Image, CancellationToken.None);
            file.IsUsing = true;
            _fileRepo.Create(file);
            await _fileRepo.SaveChangesAsync();
            return file.SourceUrl;
        }

        public async Task EndSessionAsync(SignOutCommand data)
        {
            await _jwtManager.RevokeToken(data.AccessToken);
        }

        public async Task<JwtTokenDto> RefreshToken(RefreshTokenCommand data)
        {
            var info = _jwtManager.ValidateAccessToken(data.AccessToken)
                ?? throw new TokenExpiredException();
            var user = await _userManager.FindByIdAsync(info.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty)
                ?? throw new TokenExpiredException();
            
            if (!await _jwtManager.ValidateRefreshToken(user.Id, data.RefreshToken))
                throw new TokenExpiredException();

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtManager.IssueToken(user, roles, isLogin: false);
            return token;
        }

        public async Task<List<string>> GetAllSystemRoles()
        {
            return await _roleManager.Roles
                .Select(r => r.Name!)
                .ToListAsync();
        }

        public async Task<PaginatedDto<UserListItemDto>> GetAllUsers(GetListUsersQuery request)
        {
            var filterBuilder = new FilterBuilder<AppIdentityUser>();
            if (!string.IsNullOrEmpty(request.Email))
            {
                filterBuilder.AndCondition(u => u.Email != null && u.Email.Contains(request.Email));
            }

            var query = _userManager.Users.AsNoTracking();
            var predicate = filterBuilder.GetPredicateOrDefault();
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

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
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
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
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
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

        public async Task<UpdateResponseDto> UpdateUser(UpdateUserCommand request)
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
            user.EmailConfirmed = data.EmailConfirmed;
            user.PhoneNumber = data.PhoneNumber;

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
            await _fileRepo.SaveChangesAsync();

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
            await _fileRepo.SaveChangesAsync();

            return new UpdateResponseDto
            {
                Id = user.Id,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        public async Task<Dictionary<string, string>> GetImageUrls(List<string> userIds)
        {
            return await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.ImageUrl ?? StringHelper.GetDefaultImageUrl());
        }

        public async Task<bool> SetUserStatus(string userId, bool isLockout)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            user.IsActive = !isLockout;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
