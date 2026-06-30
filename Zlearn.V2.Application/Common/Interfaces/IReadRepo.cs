using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zlearn.V2.Application.Common.DTOs;

namespace Zlearn.V2.Application.Common.Interfaces
{
    public interface IReadRepo<TDocument> where TDocument : class
    {
        Task<TDocument?> GetByIdAsync(string id);
        Task<List<TDocument>> GetAllAsync(Expression<Func<TDocument, bool>>? filter = null);
        Task<PaginatedDto<TDocument>> GetPagingAsync(int page, int size, Expression<Func<TDocument, bool>>? filter = null);
        Task<bool> AnyAsync(Expression<Func<TDocument, bool>> filter);
    }
}
