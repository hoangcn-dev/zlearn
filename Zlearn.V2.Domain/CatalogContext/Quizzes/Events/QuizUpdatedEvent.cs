using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Quizzes.Events
{
    public record QuizUpdatedEvent(
        string QuizId,
        string Name,
        string Slug,
        string CategoryId,
        bool IsPublic
    ) : DomainEvent, IModifiedAuditEvent
    {
        public string? ModifiedBy { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
    }
}
