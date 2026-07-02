using System.Collections.Generic;
using Zlearn.V2.Domain.CatalogContext.Answers;
using Zlearn.V2.Domain.CatalogContext.Quizzes;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Questions
{
    public class Question : AuditableEntity
    {
        public string Slug { get; set; } = string.Empty;
        public string? StringContent { get; set; }
        public string MediaFileUrls { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public int Order { get; set; }
        public List<Answer> Answers { get; set; } = new();
        public string QuizId { get; set; } = string.Empty;
        public Quiz? Quiz { get; set; }
        public int AttemptCount { get; set; }
    }
}
