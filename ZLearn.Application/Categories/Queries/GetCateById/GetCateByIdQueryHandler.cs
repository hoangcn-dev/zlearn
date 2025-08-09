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
            var filterBuilder = new FilterBuilder<Category>();
            if (!string.IsNullOrEmpty(request.Slug))
            {
                filterBuilder.AndCondition(c => c.Slug == request.Slug);
            }
            else if (!string.IsNullOrEmpty(request.Id))
            {
                filterBuilder.AndCondition(c => c.Id == request.Id);
            }
            else
            {
                throw new ArgumentException("Category ID or Id must be provided.");
            }
            var cate = await _cateRepo.Get(
                filter: filterBuilder.GetPredicateOrDefault(),
                projector: c => new CateDetailDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ThumbnailUrl = c.ThumbnailUrl ?? StringHelper.GetDefaultImageUrl(),
                    Slug = c.Slug,
                    Description = c.Description ?? "Chưa có mô tả",
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy,
                    LastModifiedAt = c.LastModifiedAt,
                    ModifiedBy = c.ModifiedBy,
                    QuizCount = c.Quizzes.Count,
                }) ?? throw new NotFoundException(nameof(Category), request.Id);
            return cate;
        }
    }
}
