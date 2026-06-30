using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Interfaces;

namespace ZLearn.Infras.Data.Repositories
{
    public class ReadRepo<TDocument> : IReadRepo<TDocument> where TDocument : class
    {
        protected readonly IMongoCollection<TDocument> _collection;

        public ReadRepo(IMongoDatabase database)
        {
            // Tên Collection mặc định là tên của Document Class
            // Ví dụ: CategoryDocument -> Categories
            var typeName = typeof(TDocument).Name;
            var name = typeName.EndsWith("Document") 
                ? typeName.Substring(0, typeName.Length - "Document".Length) 
                : typeName;

            string collectionName;
            if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            {
                collectionName = name.Substring(0, name.Length - 1) + "ies";
            }
            else if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase) || 
                     name.EndsWith("ch", StringComparison.OrdinalIgnoreCase) || 
                     name.EndsWith("sh", StringComparison.OrdinalIgnoreCase) || 
                     name.EndsWith("x", StringComparison.OrdinalIgnoreCase) || 
                     name.EndsWith("z", StringComparison.OrdinalIgnoreCase))
            {
                collectionName = name.EndsWith("z", StringComparison.OrdinalIgnoreCase) ? name + "zes" : name + "es";
            }
            else
            {
                collectionName = name + "s";
            }

            _collection = database.GetCollection<TDocument>(collectionName);
        }

        public virtual async Task<TDocument?> GetByIdAsync(string id)
        {
            // Giả định các Document dùng chung trường "Id" dạng string làm khóa chính
            var filter = Builders<TDocument>.Filter.Eq("Id", id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public virtual async Task<List<TDocument>> GetAllAsync(Expression<Func<TDocument, bool>>? filter = null)
        {
            if (filter == null)
            {
                return await _collection.Find(Builders<TDocument>.Filter.Empty).ToListAsync();
            }
            return await _collection.Find(filter).ToListAsync();
        }

        public virtual async Task<PaginatedDto<TDocument>> GetPagingAsync(int page, int size, Expression<Func<TDocument, bool>>? filter = null)
        {
            var mongoFilter = filter == null 
                ? Builders<TDocument>.Filter.Empty 
                : Builders<TDocument>.Filter.Where(filter);

            var totalCount = await _collection.CountDocumentsAsync(mongoFilter);
            
            var items = await _collection.Find(mongoFilter)
                .Skip((page - 1) * size)
                .Limit(size)
                .ToListAsync();

            return new PaginatedDto<TDocument>
            {
                Items = items,
                PageIndex = page,
                PageSize = size,
                TotalItems = (int)totalCount
            };
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TDocument, bool>> filter)
        {
            var count = await _collection.CountDocumentsAsync(Builders<TDocument>.Filter.Where(filter));
            return count > 0;
        }
    }
}
