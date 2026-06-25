using System.Collections.Generic;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class QuestionBankItemDto
    {
        public string Id { get; set; } = null!;
        public string? StringContent { get; set; }
        public string QuizId { get; set; } = null!;
        public string QuizName { get; set; } = null!;
        public string CategoryId { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public List<int> CorrectKeys { get; set; } = new();
        public string? Explanation { get; set; }
        public List<QuestionBankAnswerDto> Answers { get; set; } = new();
    }

    public class QuestionBankAnswerDto
    {
        public int Key { get; set; }
        public string? StringContent { get; set; }
    }
}
