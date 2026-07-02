using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Answers.Events
{
    public record AnswerDeletedEvent : DeletedEvent
    {
        public AnswerDeletedEvent(string answerId) : base(answerId)
        {
        }
    }
}
