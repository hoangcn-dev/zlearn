using Microsoft.AspNetCore.Http;
using ZLearn.Application.Files.DTOs;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Common.Services
{
    public interface IMediaStoreService
    {
        Task<bool> IsFileExist(string url, CancellationToken cancellation);
        Task<MediaFile> SaveFile(SaveFileRequestItemDto file, MediaType type, CancellationToken cancellation);
        Task<bool> RemoveFile(string url, MediaType type);
    }
}
