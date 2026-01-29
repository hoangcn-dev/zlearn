using ZLearn.Application.Common.Queries;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Queries.GetQuizzesForSitemap
{
    public class GetQuizzesForSitemapQueryHandler : BaseQueryHandler, IRequestHandler<GetQuizzesForSitemapQuery, List<QuizSitemapDto>>
    {
        private readonly IQuizRepo _quizRepo;

        public GetQuizzesForSitemapQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IQuizRepo quizRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
        }

        public async Task<List<QuizSitemapDto>> Handle(GetQuizzesForSitemapQuery request, CancellationToken cancellationToken)
        {
            var quizzes = await _quizRepo.GetAll(
                filter: q => true, // Get all quizzes
                projector: q => new QuizSitemapDto
                {
                    Slug = q.Slug,
                    CreatedAt = q.CreatedAt.DateTime,
                    UpdatedAt = q.LastModifiedAt.HasValue ? q.LastModifiedAt.Value.DateTime : (DateTime?)null
                },
                orderBy: q => q.CreatedAt,
                isAsc: false
            );

            return quizzes;
        }
    }
}
