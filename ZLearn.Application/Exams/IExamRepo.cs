using System.Security.Claims;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams
{
    public interface IExamRepo : IBaseRepo<Exam>
    {
        Task<(ExamContentDto, ExamParticipant)?> GetExamContentAsync(string examId, string userId);
        Task SaveResult(string participantId, SubmitExamDto data);
        Task<ParticipantResultDto?> GetResult(string participantId, string alias);
        Task<ParticipantWaitingInfoDto?> GetParticipantStatusAsync(string userId, string alias);
        Task<ParticipantStatus> SetParticipantStatus(string userId, string examId, ParticipantStatus status);
        Task<ParticipantWaitingInfoDto> AddParticipant(ClaimsPrincipal user, JoinExamRequestDto data);
        Task<bool> IsExamCreator(string examId, string userId);
        Task<bool> IsExamParticipant(string examId, string userId);
        Task<ParticipantStatusDto?> GetExamParticipant(string examId, string userId);
        Task<string> ManageParticipant(string examId, string participantId, ManageParticipantAction action);
        Task EndExam(string userId, string examId);
    }
}
