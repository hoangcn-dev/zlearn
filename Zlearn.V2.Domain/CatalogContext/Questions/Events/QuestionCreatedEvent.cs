using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Questions.Events
{
    public record QuestionCreatedEvent : DomainEvent
    {
        public string QuestionId { get; }
        public string Slug { get; }
        public string? StringContent { get; }
        public int Order { get; }
        public string QuizId { get; }

        public QuestionCreatedEvent(string questionId, string slug, string? stringContent, int order, string quizId)
        {
            QuestionId = questionId;
            Slug = slug;
            StringContent = stringContent;
            Order = order;
            QuizId = quizId;
        }
    }
}
