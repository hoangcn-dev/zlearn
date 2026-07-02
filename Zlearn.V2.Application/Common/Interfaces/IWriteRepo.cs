using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Application.Common.Interfaces
{
    public interface IWriteRepo<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity?> GetByIdAsync(string id);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter);
        void Create(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

