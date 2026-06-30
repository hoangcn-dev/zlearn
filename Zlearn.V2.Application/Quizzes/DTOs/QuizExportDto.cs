using System.Collections.Generic;

namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class QuizExportDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<QuestionExportDto> Questions { get; set; } = new List<QuestionExportDto>();
    }

    public class QuestionExportDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public List<AnswerExportDto> Answers { get; set; } = new List<AnswerExportDto>();
    }

    public class AnswerExportDto
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public int Key { get; set; }
        public bool IsCorrect { get; set; }
    }
}
