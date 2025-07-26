using ZLearn.Application.Common.Interfaces;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Realtime
{
    public interface IAccessHistoryRepo : IBaseRepo<AccessHistory>
    {
        Task<long> GetTotalAccessCount();
    }
}
