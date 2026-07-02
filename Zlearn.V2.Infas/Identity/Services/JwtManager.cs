using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Identity.DTOs;
using Zlearn.V2.Infas.Identity;

namespace Zlearn.V2.Infas.Identity.Services
{
    public class JwtManager
    {
        private readonly IRedisService _redisService;
        private readonly JwtConfig _jwtConfig;

        public JwtManager(
            IOptions<JwtConfig> jwtConfig,
            IRedisService redisService)
        {
            _redisService = redisService;
            _jwtConfig = jwtConfig.Value;
        }

        public JwtTokenDto IssueToken(AppIdentityUser user, IList<string> userRoleNames, bool isLogin)
        {
            // Prepare claims
            List<Claim> claims = new()
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(nameof(user.UserName), user.UserName ?? string.Empty),
                new(nameof(user.FirstName), user.FirstName ?? string.Empty),
                new(nameof(user.LastName), user.LastName ?? string.Empty),
                new(nameof(user.ImageUrl), user.ImageUrl ?? "https://res.cloudinary.com/hoangcn-dev/image/upload/v1700000000/default-avatar.png"),
                new(nameof(user.Email), user.Email ?? string.Empty),
            };
            foreach (var roleName in userRoleNames)
            {
                claims.Add(new(ClaimTypes.Role, roleName));
            }

            // Prepare token key from secret key
            byte[] bytes = Encoding.UTF8.GetBytes(EnvVariableHelper.GetValue(EnvVariableNames.JWT_SECRET_KEY));
            SymmetricSecurityKey tokenKey = new(bytes);

            // Expiration time
            DateTime expireAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ATExpirationMinutes);

            // Gen new access token & refresh token
            JwtSecurityToken token = new
            (
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                signingCredentials: new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha256),
                claims: claims,
                expires: expireAt
            );
            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Create new refresh token, if this is login => save refresh token to _redis
            string refreshToken = GenerateRefreshToken();
            if (isLogin)
            {
                _redisService.Set(
                    RedisKeys.REFRESH_TOKEN,
                    user.Id,
                    refreshToken,
                    TimeSpan.FromMinutes(_jwtConfig.RTExpirationMinutes));
            }
            else
            {
                // If this is refresh access token => update new refresh token
                _redisService.UpdateAndKeepTTL(RedisKeys.REFRESH_TOKEN, user.Id, refreshToken);
            }

            return new JwtTokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }

        private string GenerateRefreshToken()
        {
            byte[] bytes = new byte[32];
            using var random = RandomNumberGenerator.Create();
            random.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal? ValidateAccessToken(string accessToken)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(EnvVariableHelper.GetValue(EnvVariableNames.JWT_SECRET_KEY));
            SymmetricSecurityKey key = new(bytes);
            TokenValidationParameters param = new()
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtConfig.Issuer,

                ValidateAudience = true,
                ValidAudience = _jwtConfig.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,

                ValidateLifetime = false
            };

            try
            {
                ClaimsPrincipal claim = new JwtSecurityTokenHandler().ValidateToken(accessToken, param, out SecurityToken validatedToken);
                JwtSecurityToken? token = validatedToken as JwtSecurityToken;
                if (token is null || token.Header.Alg.ToLower() != SecurityAlgorithms.HmacSha256.ToLower())
                    return null;
                return claim;
            }
            catch
            {
                return null;
            }
        }

        public async Task RevokeToken(string accessToken)
        {
            await _redisService.Set(
                RedisKeys.REVOKED_ACCESS_TOKEN,
                accessToken, "-",
                TimeSpan.FromMinutes(_jwtConfig.ATExpirationMinutes));

            ClaimsPrincipal? claim = ValidateAccessToken(accessToken);
            if (claim != null)
            {
                var userId = claim.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    await _redisService.Delete(RedisKeys.REFRESH_TOKEN, userId);
                }
            }
        }

        public async Task<bool> ValidateRefreshToken(string userId, string refreshToken)
        {
            string? token = await _redisService.Get(RedisKeys.REFRESH_TOKEN, userId);
            if (token is null || !token.Equals(refreshToken))
                return false;
            return true;
        }

        public async Task<bool> IsRevokedToken(string accessToken)
        {
            return await _redisService.IsExists(RedisKeys.REVOKED_ACCESS_TOKEN, accessToken);
        }
    }
}
