namespace ZLearn.Application.Quizzes.DTOs
{
    public class QuizDetailDto : QuizListItemDto
    {
        public List<QuestionListItemDto> Questions { get; set; }
    }

    public class QuestionListItemDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public string Url => $"/questions?id={Id}";
    }
}
