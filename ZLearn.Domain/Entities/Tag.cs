using ZLearn.Domain.Common;

namespace ZLearn.Domain.Entities
{
    public class Tag : AuditableEntity
    {
        public string Name { get; set; }
        public List<Quiz> Quizzes { get; set; }
    }
}
