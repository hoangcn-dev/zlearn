using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Events.Cate;

namespace ZLearn.Application.Categories.Commands.UpdateCate
{
    public class UpdateCateCommandHandler: BaseCommandHandler, IRequestHandler<UpdateCateCommand, UpdateResponseDto>
    {
        private readonly ICateRepo _cateRepo;
        private readonly IFileRepo _fileRepo;

        public UpdateCateCommandHandler(
            IMapper mapper, IMediator mediator,
            ICateRepo cateRepo, IFileRepo fileRepo) : base(mapper, mediator)
        {
            _cateRepo = cateRepo;
            _fileRepo = fileRepo;
        }

        public async Task<UpdateResponseDto> Handle(UpdateCateCommand request, CancellationToken cancellationToken)
        {
            var cate = await _cateRepo.Get(request.CateId)
                ?? throw new NotFoundException(nameof(Category), request.CateId);

            var updateData = request.Data;
            if (updateData.Name != cate.Name && await _cateRepo.IsNameExists(updateData.Name)) 
                throw new DuplicateEntryException(nameof(Category), updateData.Name);
            if (updateData.Slug != cate.Slug && await _cateRepo.Any(c => c.Slug == updateData.Slug))
                throw new DuplicateEntryException(nameof(Category), nameof(Category.Slug));
            if (updateData.ThumbnailUrl != null)
            {
                if (cate.ThumbnailUrl != null)
                    await _fileRepo.DeleteFileByUrls(new List<string> { cate.ThumbnailUrl });
                cate.ThumbnailUrl = updateData.ThumbnailUrl;
            }

            cate.Name = updateData.Name;
            cate.Description = updateData.Description;
            cate.Slug = updateData.Slug ?? StringHelper.GenerateSlug(updateData.Name);
            cate.AddEvent(new CateUpdatedEvent(cate));
            _cateRepo.Update(cate);
            await _cateRepo.SaveChanges();

            return _mapper.Map<UpdateResponseDto>(cate);
        }
    }
}
