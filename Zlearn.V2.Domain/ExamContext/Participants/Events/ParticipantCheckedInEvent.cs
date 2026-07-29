using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.ExamContext.Participants.Events
{
    public record ParticipantCheckedInEvent : DomainEvent
    {
        public override string AggregateId => ParticipantId;
        public string ParticipantId { get; set; } = string.Empty;
        public string ExamId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTimeOffset CheckInTime { get; set; }

        public ParticipantCheckedInEvent(string participantId, string examId, string userId, DateTimeOffset checkInTime)
        {
            ParticipantId = participantId;
            ExamId = examId;
            UserId = userId;
            CheckInTime = checkInTime;
        }
    }
}
