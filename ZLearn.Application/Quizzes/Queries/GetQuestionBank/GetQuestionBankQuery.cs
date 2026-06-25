using MediatR;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Quizzes.Queries.GetQuestionBank
{
    public class GetQuestionBankQuery : PagingRequestDto, IRequest<PaginatedDto<QuestionBankItemDto>>
    {
        public string? SearchKey { get; set; }
        public string? CategoryId { get; set; }
        public string? QuizId { get; set; }
        public string? CurrentUserId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
