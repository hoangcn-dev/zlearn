using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Commands;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Domain.FileContext.MediaFiles;

namespace Zlearn.V2.Application.Files.Commands.DeleteFile
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
                var file = await _fileRepo.GetByIdAsync(fileId)
                    ?? throw new NotFoundException(nameof(MediaFile), fileId);
                removedFiles.Add(file);
            }
            foreach (var file in removedFiles)
            {
                _fileRepo.Delete(file);
            }
            await _fileRepo.SaveChangesAsync(cancellationToken);

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

