using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Application.Files.DTOs;
using Zlearn.V2.Domain.FileContext.MediaFiles;

namespace Zlearn.V2.Infas.External.CloudinaryStore
{
    public class CloudinaryService : IMediaStoreService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(ILogger<CloudinaryService> logger)
        {
            _cloudinary = new Cloudinary(new Account
            {
                Cloud = EnvVariableHelper.GetValue(EnvVariableNames.CLOUDINARY_NAME),
                ApiKey = EnvVariableHelper.GetValue(EnvVariableNames.CLOUDINARY_API_KEY),
                ApiSecret = EnvVariableHelper.GetValue(EnvVariableNames.CLOUDINARY_API_SECRET),
            });
            _cloudinary.Api.Secure = true;
            _logger = logger;
        }

        public async Task<bool> IsFileExist(string url, CancellationToken cancellation)
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var publicId = string.Join('/', segments.SkipWhile(s => s != "listene").ToArray());
            publicId = Path.ChangeExtension(publicId, null);
            var result = await _cloudinary.GetResourceAsync(new GetResourceParams(publicId)
            {
                ResourceType = ResourceType.Image
            }, cancellation);
            return !string.IsNullOrEmpty(result.PublicId);
        }

        public async Task<bool> RemoveFile(string url, MediaType type)
        {
            string prefix = type switch
            {
                MediaType.Image => "zlearn/images/",
                MediaType.Video => "zlearn/videos/",
                MediaType.Audio => "zlearn/audios/",
                _ => throw new ArgumentException("Unsupported media type", nameof(type))
            };
            var resourceType = type switch
            {
                MediaType.Image => ResourceType.Image,
                MediaType.Video => ResourceType.Video,
                MediaType.Audio => ResourceType.Video,
                _ => throw new ArgumentException("Unsupported media type", nameof(type))
            };
            string publicId = prefix + Path.GetFileNameWithoutExtension(url);
            var result = await _cloudinary.DeleteResourcesAsync(resourceType, publicId);
            return result.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<MediaFile> SaveFile(SaveFileRequestItemDto file, MediaType type, CancellationToken cancellation)
        {
            if (file.Data.Length == 0) return null!;
            using var fileStream = file.Data.OpenReadStream();
            var uploadParam = type switch {

                MediaType.Image => new ImageUploadParams
                {
                    Folder = "zlearn/images",
                    File = new FileDescription(file.Data.Name, fileStream),
                },
                MediaType.Video => new VideoUploadParams
                {
                    Folder = "zlearn/videos",
                    File = new FileDescription(file.Data.Name, fileStream),
                },
                MediaType.Audio => new VideoUploadParams
                {
                    Folder = "zlearn/audios",
                    File = new FileDescription(file.Data.Name, fileStream),
                },
                _ => throw new ArgumentException("Unsupported media type", nameof(type))
            };

            var result = await _cloudinary.UploadAsync(uploadParam);

            double? duration = null;
            if (type is MediaType.Audio or MediaType.Video)
            {
                duration = result.JsonObj["duration"]?.ToObject<double?>();
            }
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new MediaFile
                {
                    Id = IdGenerator.Generate("FIL"),
                    SourceUrl = result.SecureUrl.ToString(),
                    FileName = file.Name ?? Path.GetFileNameWithoutExtension(file.Data.FileName),
                    FileByteSize = result.Bytes,
                    Extension = Path.GetExtension(file.Data.FileName)!.ToUpper(),
                    Height = type == MediaType.Audio? null : result.Height,
                    Width = type == MediaType.Audio? null : result.Width,
                    SecDuration = duration,
                    CreatedAt = DateTimeOffset.UtcNow,
                };
            }
            _logger.LogError("{ErrorMessage}", result.Error.Message);
            throw new Exception($"Failed to upload file: {result.Error.Message}");
        }

        public async Task<MediaFile> SaveFile(Stream fileStream, string fileName, MediaType type, CancellationToken cancellation)
        {
            var uploadParam = type switch
            {
                MediaType.Image => new ImageUploadParams
                {
                    Folder = "zlearn/images",
                    File = new FileDescription(fileName, fileStream),
                },
                MediaType.Video => new VideoUploadParams
                {
                    Folder = "zlearn/videos",
                    File = new FileDescription(fileName, fileStream),
                },
                MediaType.Audio => new VideoUploadParams
                {
                    Folder = "zlearn/audios",
                    File = new FileDescription(fileName, fileStream),
                },
                _ => throw new ArgumentException("Unsupported media type", nameof(type))
            };

            var result = await _cloudinary.UploadAsync(uploadParam);

            double? duration = null;
            if (type is MediaType.Audio or MediaType.Video)
            {
                duration = result.JsonObj["duration"]?.ToObject<double?>();
            }
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new MediaFile
                {
                    Id = IdGenerator.Generate("FIL"),
                    SourceUrl = result.SecureUrl.ToString(),
                    FileName = fileName,
                    FileByteSize = result.Bytes,
                    Extension = Path.GetExtension(fileName)!.ToUpper(),
                    Height = type == MediaType.Audio ? null : result.Height,
                    Width = type == MediaType.Audio ? null : result.Width,
                    SecDuration = duration,
                    CreatedAt = DateTimeOffset.UtcNow,
                };
            }
            _logger.LogError("{ErrorMessage}", result.Error.Message);
            throw new Exception($"Failed to upload file: {result.Error.Message}");
        }
    }
}
