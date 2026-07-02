using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams.DTOs
{
    public class ParticipantWaitingInfoDto
    {
        public string ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public ParticipantStatus Status { get; set; }
        public string? ParticipantCode { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public ExamStatus ExamStatus { get; set; }
        public string? SessionToken { get; set; }
    }
}



