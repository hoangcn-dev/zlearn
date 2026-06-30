using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ZLearn.API.Exceptions;
using Zlearn.V2.Application.Common.Commands;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Domain.CatalogContext.Categories;

namespace Zlearn.V2.Application.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandler : BaseCommandHandler, IRequestHandler<UpdateCategoryCommand, CreateResponseDto>
    {
        private readonly IWriteRepo<Category> _repo;
        private readonly IFileRepo _fileRepo;

        public UpdateCategoryCommandHandler(
            IWriteRepo<Category> repo,
            IMapper mapper,
            IMediator mediator,
            IFileRepo fileRepo) : base(mapper, mediator)
        {
            _repo = repo;
            _fileRepo = fileRepo;
        }

        public async Task<CreateResponseDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var cate = await _repo.GetByIdAsync(request.Id);
            if (cate == null)
            {
                throw new NotFoundException(nameof(Category), request.Id);
            }

            var slug = StringHelper.GenerateSlug(request.Name);

            // Xử lý tệp đính kèm (nếu thay đổi ThumbnailUrl)
            if (cate.ThumbnailUrl != request.ThumbnailUrl)
            {
                if (!string.IsNullOrEmpty(cate.ThumbnailUrl))
                {
                    await _fileRepo.SetUnused(new List<string> { cate.ThumbnailUrl });
                }
                if (!string.IsNullOrEmpty(request.ThumbnailUrl))
                {
                    await _fileRepo.SetUsing(new List<string> { request.ThumbnailUrl });
                }
            }

            // Gọi hàm Update để chỉnh sửa dữ liệu và phát đi Event
            cate.Update(
                request.Name,
                slug,
                request.Description,
                request.ThumbnailUrl
            );

            _repo.Update(cate);
            await _repo.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CreateResponseDto>(cate);
        }
    }
}
