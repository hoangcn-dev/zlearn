using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.ExamContext.Exams.Events
{
    public record ExamCreatedEvent(
        string ExamId,
        string Name,
        string Alias,
        string QuizId,
        DateTimeOffset StartTime,
        DateTimeOffset? EndTime,
        string Status
    ) : DomainEvent, ICreatedAuditEvent
    {
        public string CreatedBy { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
