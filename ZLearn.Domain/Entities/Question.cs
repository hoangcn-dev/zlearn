using ZLearn.Domain.Common;

namespace ZLearn.Domain.Entities
{
    public class Question : AuditableEntity
    {
        public string? StringContent { get; set; }
        public string MediaFileIds { get; set; }
        public int CorrectKey { get; set; }
        public string? Explanation { get; set; }
        public int Order { get; set; }
        public List<Answer> Answers { get; set; }
        public string QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public int AttemptCount { get; set; }
    }
}
