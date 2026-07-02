using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Events.CategoryV2;

namespace ZLearn.Application.Categories.Commands.CreateCateV2
{
    public class CreateCategoryCommandHandlerV2 : BaseCommandHandler, IRequestHandler<CreateCategoryCommandV2, CreateResponseDto>
    {
        private readonly IWriteRepo<Category> _repo;
        private readonly IFileRepo _fileRepo;

        public CreateCategoryCommandHandlerV2(
            IMapper mapper, 
            IMediator mediator,
            IWriteRepo<Category> repo, 
            IFileRepo fileRepo) : base(mapper, mediator)
        {
            _repo = repo;
            _fileRepo = fileRepo;
        }

        public async Task<CreateResponseDto> Handle(CreateCategoryCommandV2 request, CancellationToken cancellationToken)
        {
            if (await _repo.AnyAsync(c => c.Name == request.Name))
                throw new DuplicateEntryException(nameof(Category), nameof(Category.Name));
            
            var slug = string.IsNullOrEmpty(request.Slug) 
                ? StringHelper.GenerateSlug(request.Name) 
                : request.Slug;

            if (await _repo.AnyAsync(c => c.Slug == slug))
                throw new DuplicateEntryException(nameof(Category), nameof(Category.Slug));

            if (!await _fileRepo.Any(f => f.SourceUrl == request.ThumbnailUrl))
                throw new NotFoundException(nameof(File), request.ThumbnailUrl);

            var cate = new Category
            {
                Id = IdGenerator.Generate("CAT"),
                Name = request.Name,
                Slug = slug,
                ThumbnailUrl = request.ThumbnailUrl,
                Description = request.Description
            };

            await _fileRepo.SetUsing(new List<string> { cate.ThumbnailUrl });

            // Phát sinh Domain Event dạng IDomainEvent
            cate.RaiseEvent(new CategoryCreatedEvent(cate.Id, cate.Name, cate.Slug));

            _repo.Create(cate);
            await _repo.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CreateResponseDto>(cate);
        }
    }
}
