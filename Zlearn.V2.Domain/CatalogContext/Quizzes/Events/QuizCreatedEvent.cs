using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Quizzes.Events
{
    public record QuizCreatedEvent(
        string QuizId,
        string Name,
        string Slug,
        string CategoryId,
        bool IsPublic
    ) : DomainEvent, ICreatedAuditEvent
    {
        public string CreatedBy { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
