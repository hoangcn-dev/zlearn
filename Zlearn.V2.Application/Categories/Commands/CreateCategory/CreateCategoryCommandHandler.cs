using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using ZLearn.API.Exceptions;
using Zlearn.V2.Application.Common.Commands;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Domain.CatalogContext.Categories;
using Zlearn.V2.Domain.CatalogContext.Categories.Events;

namespace Zlearn.V2.Application.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : BaseCommandHandler<Category>, IRequestHandler<CreateCategoryCommand, CreateResponseDto>
    {
        private readonly IFileRepo _fileRepo;
        private readonly IHttpContextAccessor _contextAccessor;

        public CreateCategoryCommandHandler(
            IWriteRepo<Category> writeRepo,
            IMapper mapper, 
            IMediator mediator,
            IFileRepo fileRepo,
            IHttpContextAccessor contextAccessor) : base(writeRepo, mapper, mediator)
        {
            _fileRepo = fileRepo;
            _contextAccessor = contextAccessor;
        }

        public async Task<CreateResponseDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            if (await _writeRepo.AnyAsync(c => c.Name == request.Name))
                throw new DuplicateEntryException(nameof(Category), nameof(Category.Name));
            
            var slug = string.IsNullOrEmpty(request.Slug) 
                ? StringHelper.GenerateSlug(request.Name) 
                : request.Slug;

            if (await _writeRepo.AnyAsync(c => c.Slug == slug))
                throw new DuplicateEntryException(nameof(Category), nameof(Category.Slug));

            if (!await _fileRepo.Any(f => f.SourceUrl == request.ThumbnailUrl))
                throw new NotFoundException(nameof(File), request.ThumbnailUrl);

            var userId = _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
            var now = DateTimeOffset.UtcNow;

            var cate = new Category
            {
                Id = IdGenerator.Generate("CAT"),
                Name = request.Name,
                Slug = slug,
                ThumbnailUrl = request.ThumbnailUrl,
                Description = request.Description,
                CreatedAt = now,
                CreatedBy = userId,
                LastModifiedAt = now,
                ModifiedBy = userId
            };

            await _fileRepo.SetUsing(new List<string> { cate.ThumbnailUrl });

            // Phát sinh Domain Event dạng IDomainEvent V2 với các thông tin nghiệp vụ chính
            cate.RaiseEvent(new CategoryCreatedEvent(
                cate.Id, 
                cate.Name, 
                cate.Slug, 
                cate.Description, 
                cate.ThumbnailUrl));

            _writeRepo.Create(cate);
            await _writeRepo.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CreateResponseDto>(cate);
        }
    }
}
