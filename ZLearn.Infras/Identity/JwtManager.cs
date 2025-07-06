using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ZLearn.Application.Common.Identity.DTOs;
using ZLearn.Application.Common.Utils;
using ZLearn.Infras.External.Redis;

namespace ZLearn.Infras.Identity
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

        public JwtTokenDto IssueToken(AppUser user, IList<string> userRoleNames, bool isLogin)
        {
            // Prepare claims
            List<Claim> claims = new()
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            foreach (var roleName in userRoleNames)
            {
                claims.Add(new(ClaimTypes.Role, roleName));
            }

            // Prepare token key from secret key
            byte[] bytes = Encoding.UTF8.GetBytes(EnvVariableHelper.GetValue(EnvVariableNames.JWT_SECRET_KEY));
            SymmetricSecurityKey tokenKey = new(bytes);

            // Expiration time
            DateTime expireAt = DateTime.Now.AddMinutes(_jwtConfig.ATExpirationMinutes);

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
                    user.Id.ToString(),
                    refreshToken,
                    TimeSpan.FromMinutes(_jwtConfig.RTExpirationMinutes));
            }
            else
            {
                // If this is refresh access token => update new refresh token
                _redisService.UpdateAndKeepTTL(RedisKeys.REFRESH_TOKEN, user.Id.ToString(), refreshToken);
            }

            return new JwtTokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }

        private string GenerateRefreshToken()
        {
            // Gen a random 32 chars string
            byte[] bytes = new byte[32];
            using var random = RandomNumberGenerator.Create();
            random.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal? ValidateAccessToken(string accessToken)
        {
            // Prepare params to validate expired accesstoken 
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

            // Validate
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
            // save access token to backlist
            await _redisService.Set(
                RedisKeys.REVOKED_ACCESS_TOKEN,
                accessToken, "-",
                TimeSpan.FromMinutes(_jwtConfig.ATExpirationMinutes));

            // remove refresh token to free up memory
            ClaimsPrincipal claim = ValidateAccessToken(accessToken)!;
            await _redisService.Delete(RedisKeys.REFRESH_TOKEN, claim.FindFirstValue(ClaimTypes.NameIdentifier));
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
