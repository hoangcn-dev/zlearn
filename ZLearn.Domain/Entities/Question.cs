using ZLearn.Domain.Common;
using ZLearn.Domain.Common.FileModel;

namespace ZLearn.Domain.Entities
{
    public class Question : AuditableEntity
    {
        public string? StringContent { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
        public int CorrectKey { get; set; }
        public int Order { get; set; }
        public List<Answer> Answers { get; set; }
        public string QuizId { get; set; }
        public Quiz Quiz { get; set; }
    }
}
