using System.Collections.Generic;

namespace ZLearn.Application.Quizzes.Commands.ScanQuiz
{
    public class ScanQuestionDto
    {
        public string StringContent { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public List<string> CategoryIds { get; set; } = new();
        public List<ScanAnswerDto> Answers { get; set; } = new();
    }

    public class ScanAnswerDto
    {
        public string StringContent { get; set; } = string.Empty;
        public bool IsCorrectAnswer { get; set; }
    }
}
