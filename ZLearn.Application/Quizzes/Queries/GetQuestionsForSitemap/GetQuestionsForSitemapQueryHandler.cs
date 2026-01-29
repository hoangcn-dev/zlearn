using ZLearn.Application.Common.Queries;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Queries.GetQuestionsForSitemap
{
    public class GetQuestionsForSitemapQueryHandler : BaseQueryHandler, IRequestHandler<GetQuestionsForSitemapQuery, List<QuestionSitemapDto>>
    {
        private readonly IQuestionRepo _questionRepo;

        public GetQuestionsForSitemapQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IQuestionRepo questionRepo) : base(mapper, mediator)
        {
            _questionRepo = questionRepo;
        }

        public async Task<List<QuestionSitemapDto>> Handle(GetQuestionsForSitemapQuery request, CancellationToken cancellationToken)
        {
            var questions = await _questionRepo.GetAll(
                filter: q => true, // Get all questions
                projector: q => new QuestionSitemapDto
                {
                    Slug = q.Slug,
                    CreatedAt = q.CreatedAt.DateTime,
                    UpdatedAt = q.LastModifiedAt.HasValue ? q.LastModifiedAt.Value.DateTime : (DateTime?)null
                },
                orderBy: q => q.CreatedAt,
                isAsc: false
            );

            return questions;
        }
    }
}
