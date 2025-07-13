using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Categories.Queries.GetPaginatedCate;
using ZLearn.Application.Common.Queries;

namespace ZLearn.Application.Categories.Queries.GetAllCates
{
    public class GetAllCatesQueryHandler: BaseQueryHandler, IRequestHandler<GetAllCatesQuery, IEnumerable<CateListItemDto>>
    {
        private readonly ICateRepo _repo;

        public GetAllCatesQueryHandler(
            IMapper mapper, IMediator mediator, 
            ICateRepo repo) : base(mapper, mediator)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<CateListItemDto>> Handle(GetAllCatesQuery request, CancellationToken cancellationToken)
        {
            var cates = await _repo.GetAll(
                orderBy: e => e.CreatedAt,
                isAsc: false,
                projector: e => new CateListItemDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    QuizCount = e.Quizzes.Count,
                    LastModifiedAt = e.LastModifiedAt
                });
            return cates;
        }
    }
}
