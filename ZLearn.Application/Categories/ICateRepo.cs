using ZLearn.Application.Common.Interfaces;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Categories
{
    public interface ICateRepo : IBaseRepo<Category>
    {
        Task<bool> IsNameExists(string name);
    }
}
