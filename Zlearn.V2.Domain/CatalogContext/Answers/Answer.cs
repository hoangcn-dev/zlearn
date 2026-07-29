using Zlearn.V2.Domain.CatalogContext.Questions;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Answers
{
    public class Answer : AuditableEntity
    {
        public int Key { get; set; }
        public string? StringContent { get; set; }
        public string QuestionId { get; set; } = string.Empty;
        public Question? Question { get; set; }
        public string MediaFileUrls { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }

        public Answer()
        {
            Id = IdGenerator.Generate("ANS");
        }
    }
}
