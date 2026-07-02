using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Commands;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Domain.CatalogContext.Categories;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Application.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : BaseCommandHandler<Category>, IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly IFileRepo _fileRepo;

        public DeleteCategoryCommandHandler(
            IWriteRepo<Category> writeRepo,
            IMapper mapper,
            IMediator mediator,
            IFileRepo fileRepo) : base(writeRepo, mapper, mediator)
        {
            _fileRepo = fileRepo;
        }

        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            foreach (var id in request.Ids)
            {
                var cate = await _writeRepo.GetByIdAsync(id);
                if (cate == null)
                {
                    throw new NotFoundException(nameof(Category), id);
                }

                // Ghi nhận sự kiện xóa V2 (dùng phương thức Delete() nội bộ của Category)
                cate.Delete();

                // Giải phóng tệp ảnh thumbnail (nếu có)
                if (!string.IsNullOrEmpty(cate.ThumbnailUrl))
                {
                    await _fileRepo.SetUnused(new List<string> { cate.ThumbnailUrl });
                }

                _writeRepo.Delete(cate);
            }

            await _writeRepo.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}


