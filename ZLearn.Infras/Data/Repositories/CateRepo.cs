using ZLearn.Application.Categories;
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Repositories
{
    public class CateRepo : BaseRepo<Category>, ICateRepo
    {
        public CateRepo(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> IsNameExists(string name)
        {
            return await _context.Categories.AnyAsync(c => c.Name == name);
        }
    }
}
