using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Categories.Queries.GetPaginatedCate;

namespace ZLearn.Application.Categories.Queries.GetAllCates
{
    public class GetAllCatesQueryHandler : IRequestHandler<GetAllCatesQuery, IEnumerable<CateListItemDto>>
    {
        private readonly ICateRepo _repo;
        private readonly IMapper _mapper;

        public GetAllCatesQueryHandler(ICateRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
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
