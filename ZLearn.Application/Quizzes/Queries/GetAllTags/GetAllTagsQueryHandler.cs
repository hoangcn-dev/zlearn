using ZLearn.Application.Common.Queries;

namespace ZLearn.Application.Quizzes.Queries.GetAllTags
{
    public class GetAllTagsQueryHandler : BaseQueryHandler, IRequestHandler<GetAllTagsQuery, List<string>>
    {
        private readonly IQuizRepo _quizRepo;

        public GetAllTagsQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IQuizRepo quizRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
        }

        public async Task<List<string>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
        {
            return await _quizRepo.GetAllTagsAsync();
        }
    }
}
