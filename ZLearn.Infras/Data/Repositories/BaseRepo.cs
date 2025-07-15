using System.Linq.Expressions;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Domain.Common;

namespace ZLearn.Infras.Data.Repositories
{
    public class BaseRepo<TEntity> : IBaseRepo<TEntity> where TEntity : BaseEntity
    {
        protected readonly AppDbContext _context;

        public BaseRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Any(Expression<Func<TEntity, bool>> filter)
        {
            return await _context.Set<TEntity>().AsNoTracking()
                .AnyAsync(filter);
        }

        public virtual void Create(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public void CreateRange(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().AddRange(entities);
        }

        public virtual void Delete(IEnumerable<TEntity> entities)
        {
            _context.Set<TEntity>().RemoveRange(entities);
        }

        public virtual async Task<TEntity?> Get(string id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task<TDto?> Get<TDto>(string id, Expression<Func<TEntity, TDto>> projector)
        {
            return await _context.Set<TEntity>()
                .Where(e => e.Id == id)
                .Select(projector)
                .FirstOrDefaultAsync();
        }

        public async Task<TEntity?> Get(Expression<Func<TEntity, bool>> filter)
        {
            return await _context.Set<TEntity>().FirstOrDefaultAsync(filter);
        }

        public async Task<TDto?> Get<TDto>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TDto>> projector)
        {
            return await _context.Set<TEntity>()
                .Where(filter)
                .Select(projector)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, object>>? orderBy = null, bool isAsc = true)
        {
            var query = _context.Set<TEntity>().AsQueryable().AsNoTracking();
            orderBy ??= e => e.Id;
            if (isAsc) query = query.OrderBy(orderBy);
            else query = query.OrderByDescending(orderBy);
            return await query.ToListAsync();
        }

        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>>? orderBy = null, bool isAsc = true)
        {
            var query = _context.Set<TEntity>().AsQueryable().AsNoTracking();
            query = query.Where(filter);
            orderBy ??= e => e.Id;
            if (isAsc) query = query.OrderBy(orderBy);
            else query = query.OrderByDescending(orderBy);
            return await query.ToListAsync();
        }

        public async Task<List<TDto>> GetAll<TDto>(Expression<Func<TEntity, TDto>> projector, Expression<Func<TEntity, object>>? orderBy = null, bool isAsc = true)
        {
            var query = _context.Set<TEntity>().AsQueryable().AsNoTracking();
            orderBy ??= e => e.Id;
            if (isAsc) query = query.OrderBy(orderBy);
            else query = query.OrderByDescending(orderBy);
            return await query.Select(projector).ToListAsync();
        }

        public async Task<List<TDto>> GetAll<TDto>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TDto>> projector, Expression<Func<TEntity, object>>? orderBy = null, bool isAsc = true)
        {
            var query = _context.Set<TEntity>().AsQueryable().AsNoTracking();
            query = query.Where(filter);
            orderBy ??= e => e.Id;
            if (isAsc) query = query.OrderBy(orderBy);
            else query = query.OrderByDescending(orderBy);
            return await query.Select(projector).ToListAsync();
        }

        public async Task<PaginatedDto<TDto>> GetPaging<TDto>(int page, int size, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TDto>> projector, Expression<Func<TEntity, object>>? orderBy = null, bool isAsc = true)
        {
            var query = _context.Set<TEntity>().AsQueryable().AsNoTracking();
            query = query.Where(filter);
            orderBy ??= e => e.Id;
            if (isAsc) query = query.OrderBy(orderBy);
            else query = query.OrderByDescending(orderBy);

            var totalCount = await query.CountAsync();
            var items = await query
                .Select(projector)
                .Skip((page - 1) * size)
                .Take(size).ToListAsync();

            return new PaginatedDto<TDto>
            {
                Items = items,
                PageIndex = page,
                PageSize = size,
                TotalItems = totalCount
            };
        }

        public virtual async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        public virtual void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }
    }
}
