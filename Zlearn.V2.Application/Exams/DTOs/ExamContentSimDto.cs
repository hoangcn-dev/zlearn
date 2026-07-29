using System.Collections.Generic;

namespace Zlearn.V2.Application.Exams.DTOs
{
    public class ExamContentSimDto
    {
        public string ExamId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public string QuizId { get; set; } = string.Empty;
        public string QuizName { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public List<QuestionSimDto> Questions { get; set; } = [];
    }

    public class QuestionSimDto
    {
        public string QuestionId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<AnswerSimDto> Answers { get; set; } = [];
    }

    public class AnswerSimDto
    {
        public string AnswerId { get; set; } = string.Empty;
        public int Key { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
