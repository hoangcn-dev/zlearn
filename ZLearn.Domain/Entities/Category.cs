using ZLearn.Domain.Common;

namespace ZLearn.Domain.Entities
{
    public class Category : AuditableEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public IList<Quiz> Quizzes { get; private set; } = new List<Quiz>();
    }
}
