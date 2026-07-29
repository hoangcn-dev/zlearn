using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.ExamContext.Participants.Events
{
    public record ParticipantUnbannedEvent : DomainEvent
    {
        public override string AggregateId => ParticipantId;
        public string ParticipantId { get; set; } = string.Empty;
        public string ExamId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public ParticipantUnbannedEvent(string participantId, string examId, string userId)
        {
            ParticipantId = participantId;
            ExamId = examId;
            UserId = userId;
        }
    }
}
