using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Services;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Files.Commands.DeleteFile
{
    public class DeleteFileCommandHandler : BaseCommandHandler, IRequestHandler<DeleteFileCommand, DeleteResponseDto>
    {
        private readonly IFileRepo _fileRepo;
        private readonly IMediaStoreService _mediaStoreService;

        public DeleteFileCommandHandler(
            IMapper mapper, IMediator mediator,
            IFileRepo fileRepo, 
            IMediaStoreService mediaStoreService) : base(mapper, mediator)
        {
            _fileRepo = fileRepo;
            _mediaStoreService = mediaStoreService;
        }
        public async Task<DeleteResponseDto> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            var removedFiles = new List<MediaFile>();
            foreach (var fileId in request.FileIds)
            {
                var file = await _fileRepo.Get(fileId)
                    ?? throw new NotFoundException(nameof(MediaFile), fileId);
                removedFiles.Add(file);
            }
            _fileRepo.Delete(removedFiles);
            await _fileRepo.SaveChanges();

            foreach (var file in removedFiles)
            {
                await _mediaStoreService.RemoveFile(file.SourceUrl, file.Type);
            }

            return new DeleteResponseDto
            {
                DeletedAt = DateTimeOffset.UtcNow,
                DeletedIds = removedFiles.Select(f => f.Id).ToList(),
            };
        }
    }
}
