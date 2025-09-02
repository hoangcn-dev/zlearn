using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;

namespace ZLearn.Application.Categories.Queries.GetAllCates
{
    public class GetAllCatesQueryHandler: BaseQueryHandler, IRequestHandler<GetAllCatesQuery, IEnumerable<CateListItemDto>>
    {
        private readonly ICateRepo _repo;

        public GetAllCatesQueryHandler(
            IMapper mapper, IMediator mediator,
            ICateRepo repo, IFileRepo fileRepo) : base(mapper, mediator)
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
                    Slug = e.Slug,
                    ThumbnailUrl = e.ThumbnailUrl ?? StringHelper.GetDefaultImageUrl(),
                    QuizCount = e.Quizzes.Count,
                    LastModifiedAt = e.LastModifiedAt,
                });
            return cates;
        }
    }
}
