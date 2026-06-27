using Microsoft.AspNetCore.Http;
using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Services
{
    public interface IExamSessionService
    {
        Task<string> GetUserIdAsync(HttpContext httpContext);
        Task<string?> GetSessionTokenAsync(HttpContext httpContext);
        Task HandleJoinSessionAsync(HttpContext httpContext, string userId, string examId, string participantId, ParticipantWaitingInfoDto result);
        Task HandleSubmitSessionAsync(HttpContext httpContext, string userId, string examId);
    }
}
