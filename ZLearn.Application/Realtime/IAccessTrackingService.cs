namespace ZLearn.Application.Realtime
{
    public interface IAccessTrackingService
    {
        Task<int> GetAccessCount(string ip);
        Task<int> GetAccessCount();
        Task<int> GetAccessCountToday();
        Task UpdateAccessCount();
        Task IncreaseAccess(string ip);
        Task DecreaseAccess(string ip);
    }
}
