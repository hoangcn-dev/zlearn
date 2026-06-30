using Microsoft.AspNetCore.Http;
using Zlearn.V2.Application.Files.DTOs;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace Zlearn.V2.Application.Files
{
    public interface IMediaStoreService
    {
        Task<bool> IsFileExist(string url, CancellationToken cancellation);
        Task<MediaFile> SaveFile(SaveFileRequestItemDto file, MediaType type, CancellationToken cancellation);
        Task<MediaFile> SaveFile(Stream fileStream, string fileName, MediaType type, CancellationToken cancellation);
        Task<bool> RemoveFile(string url, MediaType type);
    }
}
