namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class QuizSitemapDto
    {
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class QuestionSitemapDto
    {
        public string Slug { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

