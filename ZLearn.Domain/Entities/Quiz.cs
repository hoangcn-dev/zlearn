using ZLearn.Domain.Common;

namespace ZLearn.Domain.Entities
{
    public class Quiz : AuditableEntity
    {
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public Category Category { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Question> Questions { get; set; }
    }
}
