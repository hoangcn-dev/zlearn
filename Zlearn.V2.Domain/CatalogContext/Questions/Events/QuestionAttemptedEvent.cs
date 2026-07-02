using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Questions.Events
{
    public record QuestionAttemptedEvent : DomainEvent
    {
        public string QuestionId { get; }

        public QuestionAttemptedEvent(string questionId)
        {
            QuestionId = questionId;
        }
    }
}
