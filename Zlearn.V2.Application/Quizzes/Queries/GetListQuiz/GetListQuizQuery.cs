using MediatR;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetListQuiz
{
    public class GetListQuizQuery : PagingRequestDto, IRequest<PaginatedDto<QuizListItemDto>>
    {
        public string? Name { get; set; }
        public string? CategorySlug { get; set; }
        public string? ExcludeId { get; set; }
    }
}

