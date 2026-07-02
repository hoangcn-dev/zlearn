using System;
using System.Collections.Generic;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Quizzes.Events
{
    public record QuizUpdatedEvent(
        string QuizId,
        string Name,
        string Slug,
        string CategoryId,
        string CategoryName,
        string CategorySlug,
        bool IsPublic,
        List<QuestionPayload> Questions,
        List<string> Tags
    ) : DomainEvent, IModifiedAuditEvent
    {
        public string? ModifiedBy { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
        public override string AggregateId => QuizId;
    }
}
