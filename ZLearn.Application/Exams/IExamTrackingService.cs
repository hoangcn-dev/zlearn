using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams
{
    public interface IExamTrackingService
    {
        Task UpdateParticipantStatus(string examId, string userId, ParticipantStatus status);
        Task AddParticipant(string examId, ParticipantStatusDto participant);
    }
}
