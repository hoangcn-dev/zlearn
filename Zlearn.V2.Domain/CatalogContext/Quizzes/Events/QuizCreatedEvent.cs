using System;
using System.Collections.Generic;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Quizzes.Events
{
    public record QuizCreatedEvent(
        string QuizId,
        string Name,
        string Slug,
        string CategoryId,
        string CategoryName,
        string CategorySlug,
        bool IsPublic,
        List<QuestionPayload> Questions,
        List<string> Tags
    ) : DomainEvent, ICreatedAuditEvent
    {
        public string CreatedBy { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public override string AggregateId => QuizId;
    }
}
