using ZLearn.Domain.Common;
using ZLearn.Domain.Common.FileModel;

namespace ZLearn.Domain.Entities
{
    public class Answer : AuditableEntity
    {
        public int Key { get; set; }
        public string? StringContent { get; set; }
        public string? ImageUrl { get; set; }
        public string QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
