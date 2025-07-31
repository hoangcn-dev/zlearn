using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
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

            if (request.Name != cate.Name && await _cateRepo.IsNameExists(request.Name)) 
                throw new DuplicateEntryException(nameof(Category), request.Name);

            if (request.ThumbnailId != null)
            {
                if (cate.ThumbnailId != null)
                    await _fileRepo.DeleteFileByIds(new List<string> { cate.ThumbnailId });
                cate.ThumbnailId = request.ThumbnailId;
            }

            cate.Name = request.Name;
            cate.Description = request.Description;
            cate.AddEvent(new CateUpdatedEvent(cate));
            _cateRepo.Update(cate);
            await _cateRepo.SaveChanges();

            return _mapper.Map<UpdateResponseDto>(cate);
        }
    }
}
