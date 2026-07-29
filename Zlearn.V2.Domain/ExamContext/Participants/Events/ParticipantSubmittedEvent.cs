using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.ExamContext.Participants.Events
{
    public record ParticipantSubmittedEvent : DomainEvent
    {
        public override string AggregateId => ParticipantId;
        public string ParticipantId { get; set; } = string.Empty;
        public string ExamId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int Completed { get; set; }
        public int Correct { get; set; }
        public double Score { get; set; }
        public ParticipantStatus SubmitStatus { get; set; }
        public DateTimeOffset CheckOutTime { get; set; }

        public ParticipantSubmittedEvent(
            string participantId,
            string examId,
            string userId,
            int completed,
            int correct,
            double score,
            ParticipantStatus submitStatus,
            DateTimeOffset checkOutTime)
        {
            ParticipantId = participantId;
            ExamId = examId;
            UserId = userId;
            Completed = completed;
            Correct = correct;
            Score = score;
            SubmitStatus = submitStatus;
            CheckOutTime = checkOutTime;
        }
    }
}
