namespace ZLearn.Application.Quizzes.DTOs
{
    public class CorrectAnswerKeyDto
    {
        public string QuestionId { get; set; } = null!;
        public List<int> CorrectKeys { get; set; } = new();
        public string? Explanation { get; set; }
    }
}
