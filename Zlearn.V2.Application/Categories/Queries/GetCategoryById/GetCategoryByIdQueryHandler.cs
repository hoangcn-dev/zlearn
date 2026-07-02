using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Categories.DTOs;
using CategoryDocumentV2 = Zlearn.V2.Application.Categories.DTOs.CategoryDocument;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Queries;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Domain.CatalogContext.Categories;

namespace Zlearn.V2.Application.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdQueryHandler : BaseQueryHandler<CategoryDocumentV2>, IRequestHandler<GetCategoryByIdQuery, CateDetailDto>
    {
        public GetCategoryByIdQueryHandler(
            IReadRepo<CategoryDocumentV2> readRepo,
            IMapper mapper, 
            IMediator mediator) : base(readRepo, mapper, mediator)
        {
        }

        public async Task<CateDetailDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            CategoryDocumentV2? category = null;

            if (!string.IsNullOrEmpty(request.Id))
            {
                category = await _readRepo.GetByIdAsync(request.Id);
            }
            else if (!string.IsNullOrEmpty(request.Slug))
            {
                var list = await _readRepo.GetAllAsync(c => c.Slug == request.Slug);
                category = list.Find(c => c.Slug == request.Slug);
            }
            else
            {
                throw new ArgumentException("Category ID or Slug must be provided.");
            }

            if (category == null)
            {
                throw new NotFoundException(nameof(Category), request.Id ?? request.Slug ?? "unknown");
            }

            return new CateDetailDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description ?? "Chưa có mô tả",
                ThumbnailUrl = category.ThumbnailUrl ?? StringHelper.GetDefaultImageUrl(),
                CreatedAt = category.CreatedAt ?? DateTimeOffset.UtcNow,
                CreatedBy = category.CreatedBy,
                LastModifiedAt = category.LastModifiedAt ?? DateTimeOffset.UtcNow,
                ModifiedBy = category.ModifiedBy,
                QuizCount = category.QuizCount
            };
        }
    }
}


