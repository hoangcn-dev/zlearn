using AutoMapper;
using ZLearn.Application.Temp.Repositories;
using ZLearn.Domain.Entities;
using ZLearn.Infra.Data.Entities;

namespace ZLearn.Infra.Data.Repositories
{
    public class CategoryRepo : BaseRepo<Category, CategoryEntity>, ICategoryRepo
    {
        public CategoryRepo(AppDbContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
