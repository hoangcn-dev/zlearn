using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.ExamContext.Exams.Events
{
    public record ExamEndedEvent : DomainEvent
    {
        public override string AggregateId => ExamId;
        public string ExamId { get; set; } = string.Empty;
        public DateTimeOffset EndedAt { get; set; }

        public ExamEndedEvent(string examId, DateTimeOffset endedAt)
        {
            ExamId = examId;
            EndedAt = endedAt;
        }
    }
}
