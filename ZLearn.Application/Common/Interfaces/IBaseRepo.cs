using ZLearn.Application.Temp.DTOs;
using ZLearn.Domain.Common;

namespace ZLearn.Application.Common.Interfaces
{
    public interface IBaseRepo<TEntity> where TEntity : BaseEntity
    {
        void Create(TEntity entity);
        Task<List<TEntity>> GetAll();
        Task<TEntity?> Get(string id);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task<int> SaveChanges();
    }
}
