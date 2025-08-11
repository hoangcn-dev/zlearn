using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;

namespace ZLearn.Application.Categories.Queries.GetAllCateSlugs
{
    public class GetAllCateSlugsQueryHandler : BaseQueryHandler, IRequestHandler<GetAllCateSlugsQuery, IEnumerable<string>>
    {
        private readonly ICateRepo _repo;

        public GetAllCateSlugsQueryHandler(
            IMapper mapper,
            IMediator mediator,
            ICateRepo repo) : base(mapper, mediator)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<string>> Handle(GetAllCateSlugsQuery request, CancellationToken cancellationToken)
        {
            return await _repo.GetAll(projector: c => c.Slug);
        }
    }
}
