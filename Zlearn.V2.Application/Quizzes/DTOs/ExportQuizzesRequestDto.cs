using System.Collections.Generic;

namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class ExportQuizzesRequestDto
    {
        public List<string> QuizIds { get; set; } = new List<string>();
        public string Format { get; set; } // "word" or "pdf"
    }
}
