using ZLearn.Application.Realtime;
using ZLearn.Application.Tracking.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Repositories
{
    public class AccessHistoryRepo : BaseRepo<AccessHistory>, IAccessHistoryRepo
    {
        public AccessHistoryRepo(AppDbContext context) : base(context)
        {
        }

        public async Task<long> GetTotalAccessCount()
        {
            var count = await _context.Set<AccessHistory>()
                .SumAsync(a => a.AccessCount);
            return count;
        }
    }
}
