using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Quizzes.Queries.GetMyQuiz
{
    public class GetMyQuizQuery : IRequest<PaginatedDto<QuizListItemDto>>
    {
        public string OwnerId { get; set; }
        public QuizSearchDto Params { get; set; }
    }
}
