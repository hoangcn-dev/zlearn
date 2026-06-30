using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ZLearn.Application.Categories.DTOs;
using CategoryDocumentV2 = Zlearn.V2.Application.Categories.DTOs.CategoryDocument;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Queries;
using Zlearn.V2.Application.Common.Utils;

namespace Zlearn.V2.Application.Categories.Queries.GetAllCategories
{
    public class GetAllCategoriesQueryHandler : BaseQueryHandler<CategoryDocumentV2>, IRequestHandler<GetAllCategoriesQuery, IEnumerable<CateListItemDto>>
    {
        public GetAllCategoriesQueryHandler(
            IReadRepo<CategoryDocumentV2> readRepo,
            IMapper mapper, 
            IMediator mediator) : base(readRepo, mapper, mediator)
        {
        }

        public async Task<IEnumerable<CateListItemDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var docs = await _readRepo.GetAllAsync(c => true);
            return docs.Select(doc => new CateListItemDto
            {
                Id = doc.Id,
                Name = doc.Name,
                Slug = doc.Slug,
                ThumbnailUrl = doc.ThumbnailUrl ?? StringHelper.GetDefaultImageUrl(),
                QuizCount = doc.QuizCount,
                LastModifiedAt = doc.LastModifiedAt
            }).ToList();
        }
    }
}
