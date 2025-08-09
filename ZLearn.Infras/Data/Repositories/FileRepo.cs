using System.Threading.Tasks;
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

        public Task CheckExistingByFileUrls(HashSet<string> fileUrls)
        {
            var count = _context.Set<MediaFile>().AsNoTracking()
                .Count(file => fileUrls.Contains(file.SourceUrl));
            if (count != fileUrls.Count)
                throw new ArgumentException("Some file url do not exist in the database.");
            return Task.CompletedTask;
        }

        public async Task<Dictionary<string, MediaFile>> GetFileUrlsAsync(List<string> fileIds)
        {
            var map = await _context.Set<MediaFile>()
                .AsNoTracking()
                .Where(file => fileIds.Contains(file.Id))
                .ToDictionaryAsync(file => file.Id, file => file);
            if (map.Count != fileIds.Count)
                throw new ArgumentException("Some file IDs do not exist in the database.");
            return map;
        }

        public async Task DeleteFileByUrls(List<string> urls)
        {
            var files = await _context.Set<MediaFile>()
                .Where(file => urls.Contains(file.SourceUrl))
                .ToListAsync();
            foreach (var e in files)
            {
                await _mediaStoreService.RemoveFile(e.SourceUrl, e.Type);
            }
            base.Delete(files);
        }
    }
}
