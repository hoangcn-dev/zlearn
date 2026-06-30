using MediatR;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetQuizDetail
{
    public class GetQuizDetailQuery : IRequest<QuizDetailDto>
    {
        public string? Id { get; set; }
        public string? Slug { get; set; }
    }
}
