using ZLearn.Domain.Common;

namespace ZLearn.Domain.Entities
{
    public class Question : AuditableEntity
    {
        public string? StringContent { get; set; }
        public string ImageIds { get; set; }
        public string AudioIds { get; set; }
        public int CorrectKey { get; set; }
        public int Order { get; set; }
        public List<Answer> Answers { get; set; }
        public string QuizId { get; set; }
        public Quiz Quiz { get; set; }
    }
}
