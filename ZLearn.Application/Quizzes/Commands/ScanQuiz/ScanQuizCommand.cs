using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Quizzes.Commands.ScanQuiz
{
    public class ScanQuizCommand : IRequest<Result<List<ScanQuestionDto>>>
    {
        public IFormFile File { get; set; }
    }
}
