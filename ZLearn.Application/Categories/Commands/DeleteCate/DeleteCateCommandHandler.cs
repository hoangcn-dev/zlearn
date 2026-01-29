
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Files;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Events.Cate;

namespace ZLearn.Application.Categories.Commands.DeleteCate
{
    public class DeleteCateCommandHandler: BaseCommandHandler, IRequestHandler<DeleteCateCommand, DeleteResponseDto>
    {
        private readonly ICateRepo _cateRepo;
        private readonly IFileRepo _fileRepo;

        public DeleteCateCommandHandler(
            IMapper mapper, IMediator mediator,
            ICateRepo cateRepo, 
            IFileRepo fileRepo) : base(mapper, mediator)
        {
            _cateRepo = cateRepo;
            _fileRepo = fileRepo;
        }

        public async Task<DeleteResponseDto> Handle(DeleteCateCommand request, CancellationToken cancellationToken)
        {
            var cates = await _cateRepo
                .GetAll(filter: e => request.CateIds.Contains(e.Id));
            cates.ForEach(cate => cate.AddEvent(new CateDeletedEvent(cate)));
            _cateRepo.Delete(cates);
            var fileUrls = cates
                .Where(c => !string.IsNullOrEmpty(c.ThumbnailUrl))
                .Select(c => c.ThumbnailUrl).ToList();
            if (fileUrls.Count > 0) await _fileRepo.DeleteFileByUrls(fileUrls!);

            await _cateRepo.SaveChanges();
            return new DeleteResponseDto
            {
                DeletedIds = cates.Select(c => c.Id),
                DeletedAt = DateTime.UtcNow,
            };
        }
    }
}
