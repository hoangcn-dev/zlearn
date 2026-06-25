using AutoMapper;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Queries.GetQuestionBank
{
    public class GetQuestionBankQueryHandler : BaseQueryHandler, IRequestHandler<GetQuestionBankQuery, PaginatedDto<QuestionBankItemDto>>
    {
        private readonly IQuestionRepo _questionRepo;

        public GetQuestionBankQueryHandler(
            IMapper mapper, 
            IMediator mediator, 
            IQuestionRepo questionRepo) : base(mapper, mediator)
        {
            _questionRepo = questionRepo;
        }

        public async Task<PaginatedDto<QuestionBankItemDto>> Handle(GetQuestionBankQuery request, CancellationToken cancellationToken)
        {
            var filterBuilder = new FilterBuilder<Question>();

            // Lọc theo nội dung câu hỏi
            if (!string.IsNullOrEmpty(request.SearchKey))
            {
                filterBuilder.AndCondition(q => q.StringContent != null && q.StringContent.Contains(request.SearchKey));
            }

            // Lọc theo Category của Quiz chứa câu hỏi
            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                filterBuilder.AndCondition(q => q.Quiz.CategoryId == request.CategoryId);
            }

            // Lọc theo bộ đề thi cụ thể (Quiz)
            if (!string.IsNullOrEmpty(request.QuizId))
            {
                filterBuilder.AndCondition(q => q.QuizId == request.QuizId);
            }

            // Chỉ hiển thị câu hỏi thuộc Quiz Public hoặc Quiz do chính mình tạo
            var userId = request.CurrentUserId ?? "unknown";
            filterBuilder.AndCondition(q => q.Quiz.IsPublic || q.Quiz.CreatedBy == userId);

            var result = await _questionRepo.GetPaging(
                request.PageIndex,
                request.PageSize,
                filter: filterBuilder.GetPredicateOrDefault(),
                projector: q => new QuestionBankItemDto
                {
                    Id = q.Id,
                    StringContent = q.StringContent,
                    QuizId = q.QuizId,
                    QuizName = q.Quiz.Name,
                    CategoryId = q.Quiz.CategoryId,
                    CategoryName = q.Quiz.Category.Name,
                    CorrectKeys = q.Answers.Where(a => a.IsCorrect).Select(a => a.Key).ToList(),
                    Explanation = q.Explanation,
                    Answers = q.Answers.Select(a => new QuestionBankAnswerDto
                    {
                        Key = a.Key,
                        StringContent = a.StringContent
                    }).ToList()
                },
                orderBy: q => q.CreatedAt,
                isAsc: false
            );

            return result;
        }
    }
}
