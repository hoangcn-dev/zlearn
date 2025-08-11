using ZLearn.Application.Common.Queries;
using ZLearn.Application.Quizzes;
namespace ZLearn.Application.Quizzes.Queries.GetAllQuestionSlugs
{
    public class GetAllQuestionSlugsQueryHandler : BaseQueryHandler, IRequestHandler<GetAllQuestionSlugsQuery, IEnumerable<string>>
    {
        private readonly IQuizRepo _quizRepo;
        public GetAllQuestionSlugsQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IQuizRepo quizRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
        }

        public async Task<IEnumerable<string>> Handle(GetAllQuestionSlugsQuery request, CancellationToken cancellationToken)
        {
            var res = await _quizRepo.GetAll(projector: q => q.Questions.Select(qt => qt.Slug));
            return res.SelectMany(q => q);
        }
    }
}
