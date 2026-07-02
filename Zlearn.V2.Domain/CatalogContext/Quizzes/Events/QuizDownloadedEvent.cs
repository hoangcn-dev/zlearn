using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Quizzes.Events
{
    public record QuizDownloadedEvent : DomainEvent
    {
        public string QuizId { get; }

        public QuizDownloadedEvent(string quizId)
        {
            QuizId = quizId;
        }
    }
}
