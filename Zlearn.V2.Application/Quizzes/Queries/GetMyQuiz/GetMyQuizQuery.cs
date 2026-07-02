using MediatR;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetMyQuiz
{
    public class GetMyQuizQuery : IRequest<PaginatedDto<QuizListItemDto>>
    {
        public string OwnerId { get; set; } = string.Empty;
        public QuizSearchDto Params { get; set; } = new();
    }
}

