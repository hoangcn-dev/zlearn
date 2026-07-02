using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Questions.Events
{
    public record QuestionDeletedEvent : DeletedEvent
    {
        public QuestionDeletedEvent(string questionId) : base(questionId)
        {
        }
    }
}
