using ZLearn.Application.Common.Interfaces;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Files
{
    public interface IFileRepo : IBaseRepo<MediaFile>
    {
        /// <summary>
        /// Gets the URLs of files by their IDs. If any file ID does not exist, it will throw a NotFoundException.
        /// </summary>
        /// <param name="fileIds">Media file IDs</param>
        /// <exception cref="NotFoundException"></exception>
        /// <returns>A dictionary with keys are file id and values are url</returns>
        Task<Dictionary<string, MediaFile>> GetFileUrlsAsync(List<string> fileIds);

        /// <summary>
        /// Checks the existence of files based on their unique identifiers.
        /// </summary>
        /// <param name="fileUrls">A list of file identifiers to check. Each identifier must be a non-null, non-empty string.</param>
        /// <returns>A task that represents the asynchronous operation. The task completes when the existence check is finished.</returns>
        Task CheckExistingByFileUrls(HashSet<string> fileUrls);
        Task DeleteFileByUrls(List<string> urls);
    }
}
