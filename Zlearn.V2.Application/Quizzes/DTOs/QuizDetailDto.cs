namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class QuizDetailDto : QuizListItemDto
    {
        public List<QuestionListItemDto> Questions { get; set; }
    }

    public class QuestionListItemDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public int Order { get; set; }
        public string Url => $"/cau-hoi-trac-nghiem/{Slug}";
    }
}

