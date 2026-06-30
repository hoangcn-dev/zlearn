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
    public class GetAllCategoriesQueryHandler : BaseQueryHandler, IRequestHandler<GetAllCategoriesQuery, IEnumerable<CateListItemDto>>
    {
        private readonly IReadRepo<CategoryDocumentV2> _readRepo;

        public GetAllCategoriesQueryHandler(
            IMapper mapper, 
            IMediator mediator,
            IReadRepo<CategoryDocumentV2> readRepo) : base(mapper, mediator)
        {
            _readRepo = readRepo;
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
