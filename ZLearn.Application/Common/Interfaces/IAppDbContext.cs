using ZLearn.Domain.Entities;

namespace ZLearn.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Category> Categories { get; }
        DbSet<MediaFile> MediaFiles { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
