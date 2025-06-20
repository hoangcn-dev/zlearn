// Implementation (Tầng Infrastructure)
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZLearn.Application.Temp.DTOs;
using ZLearn.Application.Temp.Repositories;

namespace ZLearn.Infra.Data.Repositories
{
    public class BaseRepo<TModel> : IBaseRepo<TModel> where TModel : class
    {
        protected readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly Type _entityType;

        public BaseRepo(AppDbContext context, IMapper mapper, Type entityType)
        {
            _context = context;
            _mapper = mapper;
            _entityType = entityType; // Được truyền từ DI hoặc cấu hình
        }

        public async Task<CreateResultDTO> Create(TModel model)
        {
            var entity = _mapper.Map(model, typeof(TModel), _entityType)
                ?? throw new ArgumentNullException(nameof(model));
            await _context.Set(_entityType).AddAsync(entity);
            return _mapper.Map<CreateResultDTO>(entity);
        }

        public async Task<DeleteResultDTO> Delete(TModel model)
        {
            var entity = _mapper.Map(model, typeof(TModel), _entityType)
                ?? throw new ArgumentNullException(nameof(model));
            _context.Remove(entity);
            await Task.CompletedTask;
            return _mapper.Map<DeleteResultDTO>(entity);
        }

        public async Task<TModel?> Get(string id)
        {
            var entity = await _context.Set<typeof(_entityType)>().FindAsync(id);
            return entity != null ? _mapper.Map<TModel>(entity) : null;
        }

        public async Task<List<TModel>> GetAll()
        {
            var entities = await _context.Set(_entityType)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<List<TModel>>(entities);
        }

        public async Task SaveDbChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<UpdateResultDTO> Update(TModel model)
        {
            var entity = _mapper.Map(model, typeof(TModel), _entityType)
                ?? throw new ArgumentNullException(nameof(model));
            _context.Update(entity);
            await Task.CompletedTask;
            return _mapper.Map<UpdateResultDTO>(entity);
        }
    }
}