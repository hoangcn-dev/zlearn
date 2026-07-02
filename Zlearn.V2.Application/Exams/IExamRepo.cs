using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams
{
    public interface IExamRepo : IWriteRepo<Exam>
    {
        Task<Exam?> Get(System.Linq.Expressions.Expression<System.Func<Exam, bool>> filter);
        Task<TProjection?> Get<TProjection>(System.Linq.Expressions.Expression<System.Func<Exam, bool>> filter, System.Linq.Expressions.Expression<System.Func<Exam, TProjection>> projector);
        Task<List<TProjection>> GetAll<TProjection>(System.Linq.Expressions.Expression<System.Func<Exam, bool>> filter, System.Linq.Expressions.Expression<System.Func<Exam, TProjection>> projector, System.Linq.Expressions.Expression<System.Func<Exam, object>>? orderBy = null, bool isAsc = true);

        Task<(ExamContentDto, ExamParticipant)?> GetExamContentAsync(string examId, string userId);
        Task SaveResult(string participantId, SubmitExamDto data);
        Task<ParticipantResultDto?> GetResult(string participantId, string alias);
        Task<ParticipantWaitingInfoDto?> GetParticipantStatusAsync(string userId, string alias);
        Task<Zlearn.V2.Domain.ExamContext.Participants.ParticipantStatus> SetParticipantStatus(string userId, string examId, Zlearn.V2.Domain.ExamContext.Participants.ParticipantStatus status);
        Task<ParticipantWaitingInfoDto> AddParticipant(ClaimsPrincipal user, JoinExamRequestDto data);
        Task<bool> IsExamCreator(string examId, string userId);
        Task<bool> IsExamParticipant(string examId, string userId);
        Task<ParticipantStatusDto?> GetExamParticipant(string examId, string userId);
        Task<string> ManageParticipant(string examId, string participantId, ManageParticipantAction action);
        Task EndExam(string userId, string examId);
        Task<List<OnGoingExamListItemDto>> GetOnGoingExams(string userId);
        Task<FileDataDto> GetExamScoreAsExcel(string examId, string userId);
        Task<Dictionary<string, List<int>>> GetExamGradingKeysAsync(string examId);
        Task<string?> GetSelectedAnswersAsync(string examId, string userId);
        Task FinalizeExamIfAllCompletedAsync(string examId);
    }
}


