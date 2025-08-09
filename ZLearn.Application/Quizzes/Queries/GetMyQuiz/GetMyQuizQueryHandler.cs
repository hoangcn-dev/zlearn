using System.Linq.Expressions;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Queries.GetMyQuiz
{
    public class GetMyQuizQueryHandler : BaseQueryHandler, IRequestHandler<GetMyQuizQuery, PaginatedDto<QuizListItemDto>>
    {
        private readonly IQuizRepo _quizRepo;

        public GetMyQuizQueryHandler(
            IMapper mapper, 
            IMediator mediator, 
            IQuizRepo quizRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
        }

        public async Task<PaginatedDto<QuizListItemDto>> Handle(GetMyQuizQuery request, CancellationToken cancellationToken)
        {
            var filterBuilder = new FilterBuilder<Quiz>();
            filterBuilder.AndCondition(q => q.CreatedBy == request.OwnerId);
            if (!string.IsNullOrEmpty(request.Params.Name)) filterBuilder.AndCondition(q => q.Name.Contains(request.Params.Name));
            if (!string.IsNullOrEmpty(request.Params.CategoryId)) filterBuilder.AndCondition(q => q.CategoryId == request.Params.CategoryId);
            if (!string.IsNullOrEmpty(request.Params.Tag)) filterBuilder.AndCondition(q => q.Tags.Any(t => t.Name == request.Params.Tag));
            
            Expression<Func<Quiz, object>> orderBy = request.Params.OrderBy switch
            {
                nameof(Question.AttemptCount) => q => q.Questions.Select(q => q.AttemptCount).Count(),
                nameof(Quiz.CreatedAt) => q => q.CreatedAt,
                _ => q => q.CreatedAt
            };

            var quizzes = await _quizRepo.GetPaging(
                page: request.Params.PageIndex,
                size: request.Params.PageSize,
                filter: filterBuilder.GetPredicateOrDefault(),
                projector: q => new QuizListItemDto
                {
                    Id = q.Id,
                    Name = q.Name,
                    Slug = q.Slug,
                    AttemptCount = q.Questions.Select(qu => qu.AttemptCount).Sum(),
                    DownloadCount = q.DownloadCount,
                    CategoryId = q.CategoryId,
                    CategoryName = q.Category.Name,
                    QuestionCount = q.Questions.Count
                },
                orderBy: orderBy,
                isAsc: false);

            return quizzes;
        }
    }
}
