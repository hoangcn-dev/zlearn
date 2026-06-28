using System.Collections.Generic;
using ZLearn.Application.Quizzes.DTOs;
using MediatR;

namespace ZLearn.Application.Quizzes.Queries.ExportQuizzes
{
    public class ExportQuizzesQuery : IRequest<ExportDocumentDto>
    {
        public List<string> QuizIds { get; set; } = new List<string>();
        public string Format { get; set; } // "word" or "pdf"
    }
}
