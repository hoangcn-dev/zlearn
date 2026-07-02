using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Infas.Data.Repositories
{
    public class WriteRepo<TEntity> : IWriteRepo<TEntity> where TEntity : BaseEntity
    {
        protected readonly AppDbContext _context;

        public WriteRepo(AppDbContext context)
        {
            _context = context;
        }

        public virtual async Task<TEntity?> GetByIdAsync(string id)
        {
            return await _context.Set<TEntity>().FindAsync(new object[] { id.ToUpper() });
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await _context.Set<TEntity>().AnyAsync(filter);
        }

        public virtual void Create(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
