using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Domain.FileContext.MediaFiles;

namespace Zlearn.V2.Infas.Data.Repositories
{
    public class FileRepo : WriteRepo<MediaFile>, IFileRepo
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
            _context.Set<MediaFile>().RemoveRange(files);
        }

        public async Task<int> Cleanup(TimeSpan limit)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var filesToRemove = await _context.Set<MediaFile>()
                .Where(file => !file.IsUsing)
                .ToListAsync();
                filesToRemove = filesToRemove.Where(file => DateTimeOffset.UtcNow.Subtract(file.CreatedAt) > limit).ToList();
                await DeleteFileByUrls(filesToRemove.Select(f => f.SourceUrl).ToList());
                _context.MediaFiles.RemoveRange(filesToRemove);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return filesToRemove.Count;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task SetUsing(List<string> urls)
        {
            var files = _context.Set<MediaFile>()
                .Where(file => urls.Contains(file.SourceUrl))
                .ToList();
            foreach (var file in files)
            {
                file.IsUsing = true;
            }
            _context.MediaFiles.UpdateRange(files);
            await _context.SaveChangesAsync();
        }

        public async Task SetUnused(List<string> urls)
        {
            var files = _context.Set<MediaFile>()
                .Where(file => urls.Contains(file.SourceUrl))
                .ToList();
            foreach (var file in files)
            {
                file.IsUsing = false;
            }
            _context.MediaFiles.UpdateRange(files);
            await _context.SaveChangesAsync();
        }
    }
}
