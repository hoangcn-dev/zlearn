using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.ExamContext.Exams.Events
{
    public record ExamStartedEvent : DomainEvent
    {
        public override string AggregateId => ExamId;
        public string ExamId { get; set; } = string.Empty;
        public DateTimeOffset StartedAt { get; set; }

        public ExamStartedEvent(string examId, DateTimeOffset startedAt)
        {
            ExamId = examId;
            StartedAt = startedAt;
        }
    }
}
