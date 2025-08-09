using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Quizzes.Queries.GetListQuiz
{
    public class GetListQuizQuery : PagingRequestDto, IRequest<PaginatedDto<QuizListItemDto>>
    {
        public string? Name { get; set; }
        public string? CategorySlug { get; set; }
    }
}
