using ZLearn.Domain.Common;

namespace ZLearn.Domain.Entities
{
    public class Quiz : AuditableEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string CategoryId { get; set; }
        public int DownloadCount { get; set; }
        public bool IsPublic { get; set; }
        public Category Category { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<Exam> Exams { get; set; } = new List<Exam>();
    }
}
