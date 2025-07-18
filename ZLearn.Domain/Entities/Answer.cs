using ZLearn.Domain.Common;

namespace ZLearn.Domain.Entities
{
    public class Answer : AuditableEntity
    {
        public int Key { get; set; }
        public string? StringContent { get; set; }
        public string QuestionId { get; set; }
        public Question Question { get; set; }
        public string MediaFileIds { get; set; }
    }
}
