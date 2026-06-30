using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Common.Interfaces
{
    public interface IReadRepo<TDocument> where TDocument : class
    {
        Task<TDocument?> GetByIdAsync(string id);
        Task<List<TDocument>> GetAllAsync(Expression<Func<TDocument, bool>>? filter = null);
        Task<PaginatedDto<TDocument>> GetPagingAsync(int page, int size, Expression<Func<TDocument, bool>>? filter = null);
        Task<bool> AnyAsync(Expression<Func<TDocument, bool>> filter);
    }
}
