namespace ZLearn.Application.Quizzes.DTOs
{
    public class CorrectAnswerKeyDto
    {
        public string QuestionId { get; set; }
        public int CorrectKey { get; set; }
        public string Explanation { get; set; }
    }
}
