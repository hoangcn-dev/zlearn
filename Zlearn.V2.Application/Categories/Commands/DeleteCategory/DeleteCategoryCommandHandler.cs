using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ZLearn.API.Exceptions;
using Zlearn.V2.Application.Common.Commands;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Domain.CatalogContext.Categories;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Application.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : BaseCommandHandler, IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly IWriteRepo<Category> _repo;
        private readonly IFileRepo _fileRepo;

        public DeleteCategoryCommandHandler(
            IWriteRepo<Category> repo,
            IMapper mapper,
            IMediator mediator,
            IFileRepo fileRepo) : base(mapper, mediator)
        {
            _repo = repo;
            _fileRepo = fileRepo;
        }

        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            foreach (var id in request.Ids)
            {
                var cate = await _repo.GetByIdAsync(id);
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

                _repo.Delete(cate);
            }

            await _repo.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
