using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ZLearn.API.Exceptions;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.Queries;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Categories.Queries.GetCategoryByIdV2
{
    public class GetCategoryByIdQueryHandlerV2 : BaseQueryHandler, IRequestHandler<GetCategoryByIdQueryV2, CategoryDocument>
    {
        private readonly IReadRepo<CategoryDocument> _readRepo;

        public GetCategoryByIdQueryHandlerV2(
            IMapper mapper, 
            IMediator mediator,
            IReadRepo<CategoryDocument> readRepo) : base(mapper, mediator)
        {
            _readRepo = readRepo;
        }

        public async Task<CategoryDocument> Handle(GetCategoryByIdQueryV2 request, CancellationToken cancellationToken)
        {
            CategoryDocument? category = null;

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

            return category;
        }
    }
}
