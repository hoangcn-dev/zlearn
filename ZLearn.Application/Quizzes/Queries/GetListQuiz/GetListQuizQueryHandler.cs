using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Queries.GetListQuiz
{
    public class GetListQuizQueryHandler : BaseQueryHandler, IRequestHandler<GetListQuizQuery, PaginatedDto<QuizListItemDto>>
    {
        private readonly IQuizRepo _quizRepo;

        public GetListQuizQueryHandler(
            IMapper mapper, IMediator mediator, IQuizRepo quizRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
        }

        public async Task<PaginatedDto<QuizListItemDto>> Handle(GetListQuizQuery request, CancellationToken cancellationToken)
        {
            var filterBuilder = new FilterBuilder<Quiz>();
            if (!string.IsNullOrEmpty(request.Name))
            {
                filterBuilder.AndCondition(q => q.Name.Contains(request.Name));
            }
            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                filterBuilder.AndCondition(q => q.CategoryId == request.CategoryId);
            }

            var quizzes = await _quizRepo.GetPaging(
                request.PageIndex, 
                request.PageSize,
                filter: filterBuilder.GetPredicateOrDefault(),
                projector: q => new QuizListItemDto
                {
                    Id = q.Id,
                    Name = q.Name,
                    AttemptCount = q.Questions.Select(qu => qu.AttemptCount).Sum(),
                    CategoryId = q.CategoryId,
                    CategoryName = q.Category.Name,
                    QuestionCount = q.Questions.Count
                },
                isAsc: false,
                orderBy: p => p.CreatedAt);
            return quizzes;
        }
    }
}
