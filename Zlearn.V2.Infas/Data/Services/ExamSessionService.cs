using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Exams.DTOs;

namespace Zlearn.V2.Infas.Data.Services
{
    public class ExamSessionService : IExamSessionService
    {
        private readonly IRedisService _redisService;

        public ExamSessionService(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<string> GetUserIdAsync(HttpContext httpContext)
        {
            var token = httpContext.Request.Cookies["exam_session_token"];
            string? userId = null;

            if (!string.IsNullOrEmpty(token))
            {
                var session = await _redisService.GetObject<ExamSessionDto>(RedisKeys.EXAM_SESSION, token);
                if (session is not null)
                {
                    userId = session.u;
                }
            }

            if (string.IsNullOrEmpty(userId))
            {
                userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            return userId;
        }

        public async Task<string?> GetSessionTokenAsync(HttpContext httpContext)
        {
            var token = httpContext.Request.Cookies["exam_session_token"];
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var session = await _redisService.GetObject<ExamSessionDto>(RedisKeys.EXAM_SESSION, token);
            if (session is null)
            {
                return null;
            }

            return token;
        }

        public async Task HandleJoinSessionAsync(HttpContext httpContext, string userId, string examId, string participantId, ParticipantWaitingInfoDto result)
        {
            var sessionToken = Guid.NewGuid().ToString("N");
            var oldUserSessionKey = $"{userId}:{examId}";
            var oldToken = await _redisService.Get(RedisKeys.EXAM_USER_SESSION, oldUserSessionKey);
            if (!string.IsNullOrEmpty(oldToken))
            {
                await _redisService.Delete(RedisKeys.EXAM_SESSION, oldToken);
            }

            var ttl = TimeSpan.FromHours(12);
            if (result.EndTime.HasValue)
            {
                var remaining = result.EndTime.Value - DateTimeOffset.UtcNow;
                if (remaining.TotalSeconds > 0)
                {
                    ttl = remaining;
                }
                else
                {
                    ttl = TimeSpan.FromSeconds(1);
                }
            }

            var sessionPayload = new ExamSessionDto
            {
                u = userId,
                e = examId,
                p = participantId
            };
            await _redisService.SetObject(RedisKeys.EXAM_SESSION, sessionToken, sessionPayload, ttl);
            await _redisService.Set(RedisKeys.EXAM_USER_SESSION, oldUserSessionKey, sessionToken, ttl);

            result.SessionToken = sessionToken;

            httpContext.Response.Cookies.Append("exam_session_token", sessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.Add(ttl)
            });
        }

        public async Task HandleSubmitSessionAsync(HttpContext httpContext, string userId, string examId)
        {
            var token = httpContext.Request.Cookies["exam_session_token"];
            if (!string.IsNullOrEmpty(token))
            {
                await _redisService.Delete(RedisKeys.EXAM_SESSION, token);
                var userSessionKey = $"{userId}:{examId}";
                await _redisService.Delete(RedisKeys.EXAM_USER_SESSION, userSessionKey);
                httpContext.Response.Cookies.Delete("exam_session_token");
            }
        }
    }
}
