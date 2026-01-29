using ZLearn.Application.Common.Interfaces;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Files
{
    public interface IFileRepo : IBaseRepo<MediaFile>
    {
        Task<Dictionary<string, MediaFile>> GetFileUrlsAsync(List<string> fileIds);
        Task CheckExistingByFileUrls(HashSet<string> fileUrls);
        Task DeleteFileByUrls(List<string> urls);
        Task SetUsing(List<string> urls);
        Task<int> Cleanup(TimeSpan limit);
    }
}
