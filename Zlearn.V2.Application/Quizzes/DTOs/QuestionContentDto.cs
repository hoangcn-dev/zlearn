using ZLearn.Domain.Entities;

namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class QuestionContentDto
    {
        public string Id { get; set; }
        public string QuizId { get; set; }
        public string QuizName { get; set; }
        public string QuizSlug { get; set; }
        public int Order { get; set; }
        public string Slug { get; set; }
        public int AttemptCount { get; set; }
        public string? StringContent { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<string> AudioUrls { get; set; } = new List<string>();
        public List<string> VideoUrls { get; set; } = new List<string>();
        public List<AnswerContentDto> Answers { get; set; } = new List<AnswerContentDto>();
        public bool IsMultipleChoice { get; set; }
    }

    public class AnswerContentDto
    {
        public int Key { get; set; }
        public string Label { get; set; }
        public string? StringContent { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
