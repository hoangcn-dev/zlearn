using ZLearn.API.Exceptions;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Categories.Queries.GetCateById
{
    public class GetCateByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetCateByIdQuery, CateDetailDto>
    {
        private readonly ICateRepo _cateRepo;
        private readonly IFileRepo _fileRepo;

        public GetCateByIdQueryHandler(
            IMapper mapper, IMediator mediator,
            ICateRepo cateRepo, IFileRepo fileRepo) : base(mapper, mediator)
        {
            _cateRepo = cateRepo;
            _fileRepo = fileRepo;
        }

        public async Task<CateDetailDto> Handle(GetCateByIdQuery request, CancellationToken cancellationToken)
        {
            var cate = await _cateRepo.Get(
                id: request.CateId,
                projector: c => new CateDetailDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ThumbnailUrl = c.ThumbnailId ?? string.Empty,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy,
                    LastModifiedAt = c.LastModifiedAt,
                    ModifiedBy = c.ModifiedBy,
                    QuizCount = c.Quizzes.Count,
                }) ?? throw new NotFoundException(nameof(Category), request.CateId);
            if (string.IsNullOrEmpty(cate.ThumbnailUrl))
            {
                cate.ThumbnailUrl = StringHelper.GetDefaultImageUrl();
            }
            else
            {
                cate.ThumbnailUrl = (await _fileRepo.Get(cate.ThumbnailUrl)
                    ?? throw new InternalErrorException()).SourceUrl;
            }
            return cate;
        }
    }
}
