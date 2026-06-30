using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Quizzes.Events
{
    public record QuizDeletedEvent : DeletedEvent
    {
        public QuizDeletedEvent(string Id) : base(Id)
        {
        }
    }
}
