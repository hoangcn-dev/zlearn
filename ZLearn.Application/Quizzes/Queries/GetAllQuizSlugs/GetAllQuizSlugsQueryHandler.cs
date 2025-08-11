using ZLearn.Application.Common.Queries;

namespace ZLearn.Application.Quizzes.Queries.GetAllQuizSlugs
{
    public class GetAllQuizSlugsQueryHandler : BaseQueryHandler, IRequestHandler<GetAllQuizSlugsQuery, IEnumerable<string>>
    {
        private readonly IQuizRepo _quizRepo;

        public GetAllQuizSlugsQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IQuizRepo quizRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
        }
        public async Task<IEnumerable<string>> Handle(GetAllQuizSlugsQuery request, CancellationToken cancellationToken)
        {
            return await _quizRepo.GetAll(projector: c => c.Slug);
        }
    }
}
