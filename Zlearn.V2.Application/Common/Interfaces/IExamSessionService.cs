using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Zlearn.V2.Application.Exams.DTOs;

namespace Zlearn.V2.Application.Common.Interfaces
{
    public interface IExamSessionService
    {
        Task<string> GetUserIdAsync(HttpContext httpContext);
        Task<string?> GetSessionTokenAsync(HttpContext httpContext);
        Task HandleJoinSessionAsync(HttpContext httpContext, string userId, string examId, string participantId, ParticipantWaitingInfoDto result);
        Task HandleSubmitSessionAsync(HttpContext httpContext, string userId, string examId);
    }
}
