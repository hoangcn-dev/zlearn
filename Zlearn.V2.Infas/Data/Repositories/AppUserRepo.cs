using Zlearn.V2.Application.Identity;

namespace Zlearn.V2.Infas.Data.Repositories
{
    public class AppUserRepo : IAppUserRepo
    {
        private readonly AppDbContext _context;

        public AppUserRepo(AppDbContext context)
        {
            _context = context;
        }
    }
}
