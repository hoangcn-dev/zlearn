using System.Threading.Tasks;
using ZLearn.Application.Common.Services;
using ZLearn.Application.Files;
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Repositories
{
    public class FileRepo : BaseRepo<MediaFile>, IFileRepo
    {
        private readonly IMediaStoreService _mediaStoreService;

        public FileRepo(AppDbContext context, 
            IMediaStoreService mediaStoreService) : base(context)
        {
            _mediaStoreService = mediaStoreService;
        }

        public Task CheckExistingByFileIds(HashSet<string> fileIds)
        {
            var count = _context.Set<MediaFile>()
                .AsNoTracking()
                .Count(file => fileIds.Contains(file.Id));
            if (count != fileIds.Count)
                throw new ArgumentException("Some file IDs do not exist in the database.");
            return Task.CompletedTask;
        }

        public Task<Dictionary<string, string>> GetFileUrlsAsync(List<string> fileIds)
        {
            var map = _context.Set<MediaFile>()
                .AsNoTracking()
                .Where(file => fileIds.Contains(file.Id))
                .ToDictionaryAsync(file => file.Id, file => file.SourceUrl);
            if (map.Result.Count != fileIds.Count)
                throw new ArgumentException("Some file IDs do not exist in the database.");
            return map;
        }

        public async Task DeleteFileByIds(IEnumerable<string> ids)
        {
            var files = await _context.Set<MediaFile>()
                .Where(file => ids.Contains(file.Id))
                .ToListAsync();
            foreach (var e in files)
            {
                await _mediaStoreService.RemoveFile(e.SourceUrl, e.Type);
            }
            base.Delete(files);
        }
    }
}
