using System.Security.Claims;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams
{
    public interface IExamRepo : IBaseRepo<Exam>
    {
        Task<ExamContentDto?> GetExamContentAsync(string examId, string userId);
        Task SaveResult(string participantId, SubmitExamDto data);
        Task<ParticipantResultDto?> GetResult(string participantId, string alias);
        Task<ParticipantWaitingInfoDto?> GetParticipantStatusAsync(string userId, string alias);
        Task<ParticipantWaitingInfoDto> AddParticipant(ClaimsPrincipal user, JoinExamRequestDto data);
    }
}
