using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.Services;
using ZLearn.Application.Files.DTOs;
using ZLearn.Domain.Constants;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Files.Commands.SaveFile
{
    public class SaveFilesCommandHandler : BaseCommandHandler, IRequestHandler<SaveFilesCommand, ListSavedFileDto>
    {
        private readonly IMediaStoreService _mediaStoreService;
        private readonly IFileRepo _fileRepo;

        public SaveFilesCommandHandler(
            IMapper mapper, IMediator mediator,
            IMediaStoreService mediaStoreService,
            IFileRepo fileRepo) : base(mapper, mediator)
        {
            _mediaStoreService = mediaStoreService;
            _fileRepo = fileRepo;
        }

        public async Task<ListSavedFileDto> Handle(SaveFilesCommand request, CancellationToken cancellationToken)
        {
            var errors = new Dictionary<string, string>();
            var mediaFiles = new List<MediaFile>();
            try
            {
                if (request.Files == null || request.Files.Count == 0)
                {
                    throw new ValidationException("No files were uploaded.");
                }

                foreach ( var file in request.Files)
                {
                    try
                    {
                        var mediaType = ValidateMediaFile(file.Data);
                        var mediaFile = await _mediaStoreService.SaveFile(file, mediaType, cancellationToken);
                        mediaFile.Type = mediaType;
                        mediaFiles.Add(mediaFile);
                    } 
                    catch (Exception ex)
                    {
                        errors[file.Name ?? file.Data.FileName] = ex.Message;
                    }
                }

                if (mediaFiles.Count != 0)
                {
                    _fileRepo.CreateRange(mediaFiles);
                    await _fileRepo.SaveChanges();
                }
                return new ListSavedFileDto
                {
                    Errors = errors,
                    Files = mediaFiles.Select(mf => _mapper.Map<SavedFileDto>(mf)).ToList()
                };
            }
            catch (Exception ex)
            {
                foreach (var savedFile in mediaFiles)
                {
                    await _mediaStoreService.RemoveFile(savedFile.SourceUrl, savedFile.Type);
                }
                throw new Exception("An error occurred while saving files.", ex);
            }
        }

        private MediaType ValidateMediaFile(IFormFile file)
        {
            if (file.FileName.Length > StringLengths.FileNameMaxLength)
            {
                throw new ValidationException($"File name exceeds the maximum allowed length of {StringLengths.FileNameMaxLength} characters.");
            }

            var mediaType = Path.GetExtension(file.FileName) switch
            {
                FileRules.Extension.IMAGE_PNG_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_JPEG_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_JPG_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_GIF_EXTENSION => MediaType.Image,
                FileRules.Extension.IMAGE_SVG_EXTENSION => MediaType.Image,
                FileRules.Extension.AUDIO_MP3_EXTENSION => MediaType.Audio,
                FileRules.Extension.AUDIO_WAV_EXTENSION => MediaType.Audio,
                FileRules.Extension.AUDIO_OGG_EXTENSION => MediaType.Audio,
                FileRules.Extension.VIDEO_MP4_EXTENSION => MediaType.Video,
                _ => throw new ArgumentException("Unsupported file extension."),
            };

            var maxSize = mediaType switch
            {
                MediaType.Image => FileRules.MaxSize.MAX_IMAGE_FILE_SIZE_MB * 1024 * 1024,
                MediaType.Audio => FileRules.MaxSize.MAX_AUDIO_FILE_SIZE_MB * 1024 * 1024,
                MediaType.Video => FileRules.MaxSize.MAX_VIDEO_FILE_SIZE_MB * 1024 * 1024,
                _ => throw new ArgumentException("Invalid media type.")
            };
            if (file.Length > maxSize)
            {
                throw new ValidationException($"File size exceeds the maximum allowed limit for {mediaType} ({maxSize} MB).");
            }

            return mediaType;
        }
    }
}
