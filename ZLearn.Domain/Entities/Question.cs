using ZLearn.Domain.Common;

namespace ZLearn.Domain.Entities
{
    public class Question : AuditableEntity
    {
        public string Slug { get; set; }
        public string? StringContent { get; set; }
        public string MediaFileUrls { get; set; }
        public string? Explanation { get; set; }
        public int Order { get; set; }
        public List<Answer> Answers { get; set; }
        public string QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public int AttemptCount { get; set; }
    }
}
