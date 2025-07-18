using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;

namespace ZLearn.Application.Categories.Queries.GetAllCates
{
    public class GetAllCatesQueryHandler: BaseQueryHandler, IRequestHandler<GetAllCatesQuery, IEnumerable<CateListItemDto>>
    {
        private readonly ICateRepo _repo;
        private readonly IFileRepo _fileRepo;

        public GetAllCatesQueryHandler(
            IMapper mapper, IMediator mediator,
            ICateRepo repo, IFileRepo fileRepo) : base(mapper, mediator)
        {
            _repo = repo;
            _fileRepo = fileRepo;
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
                    ThumbnailUrl = e.ThumbnailId ?? string.Empty,
                    QuizCount = e.Quizzes.Count,
                    LastModifiedAt = e.LastModifiedAt,
                    AttemptCount = e.Quizzes.Select(q => q.Questions.Select(qu => qu.AttemptCount).Sum()).Sum()
                });

            // Get file urls from thumbnail ids in categories
            var thumbnailUrls = await _fileRepo.GetFileUrlsAsync(cates
                .Where(c => !string.IsNullOrEmpty(c.ThumbnailUrl))
                .Select(c => c.ThumbnailUrl).ToList());
            foreach (var cate in cates)
            {
                if (string.IsNullOrEmpty(cate.ThumbnailUrl) || !thumbnailUrls.ContainsKey(cate.ThumbnailUrl))
                {
                    cate.ThumbnailUrl = StringHelper.GetDefaultImageUrl();
                }
                else
                {
                    cate.ThumbnailUrl = thumbnailUrls[cate.ThumbnailUrl].SourceUrl;
                }
            }

            return cates;
        }
    }
}
