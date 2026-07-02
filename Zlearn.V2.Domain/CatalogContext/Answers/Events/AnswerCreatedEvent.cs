using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Answers.Events
{
    public record AnswerCreatedEvent : DomainEvent
    {
        public string AnswerId { get; }
        public int Key { get; }
        public string? StringContent { get; }
        public bool IsCorrect { get; }
        public string QuestionId { get; }

        public override string AggregateId => AnswerId;

        public AnswerCreatedEvent(string answerId, int key, string? stringContent, bool isCorrect, string questionId)
        {
            AnswerId = answerId;
            Key = key;
            StringContent = stringContent;
            IsCorrect = isCorrect;
            QuestionId = questionId;
        }
    }
}
