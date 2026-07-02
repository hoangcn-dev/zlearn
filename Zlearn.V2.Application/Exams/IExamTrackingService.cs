using System.Threading.Tasks;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams
{
    public interface IExamTrackingService
    {
        Task UpdateParticipantStatus(string examId, string userId, ParticipantStatus status);
        Task UpdateExamStatus(string examId, ExamStatus status);
        Task UpdateRemainingTime(string examId, long remainingMilisec, string methodName);
        Task AddParticipant(string examId, ParticipantStatusDto participant);
        Task UpdateParticipantProgress(string examId, string userId, int completedCount);
    }
}

