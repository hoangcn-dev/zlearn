using System.Linq.Expressions;
using ZLearn.Application.Common.DTOs;
using ZLearn.Domain.Common;

namespace ZLearn.Application.Common.Interfaces
{
    public interface IBaseRepo<TEntity> where TEntity : BaseEntity
    {
        Task<bool> Any(Expression<Func<TEntity, bool>> filter);
        void Create(TEntity entity);
        void CreateRange(IEnumerable<TEntity> entities);
        Task<List<TEntity>> GetAll(Expression<Func<TEntity, object>>? orderBy = null, bool isAsc = true);
        Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>>? orderBy = null, bool isAsc = true);
        Task<List<TDto>> GetAll<TDto>(Expression<Func<TEntity, TDto>> projector, Expression<Func<TEntity, object>>? orderBy = null, bool isAsc = true);
        Task<List<TDto>> GetAll<TDto>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TDto>> projector, Expression<Func<TEntity, object>>? orderBy = null, bool isAsc = true);
        Task<PaginatedDto<TDto>> GetPaging<TDto>(int page, int size, Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TDto>> projector, Expression<Func<TEntity, object>>? orderBy = null, bool isAsc = true);
        Task<TEntity?> Get(string id);
        Task<TDto?> Get<TDto>(string id, Expression<Func<TEntity, TDto>> projector);
        Task<TEntity?> Get(Expression<Func<TEntity, bool>> filter);
        Task<TDto?> Get<TDto>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TDto>> projector);
        void Update(TEntity entity);
        void Delete(IEnumerable<TEntity> entities);
        Task<int> SaveChanges();
    }
}
