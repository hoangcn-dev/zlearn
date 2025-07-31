using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ZLearn.Application.Realtime;
using ZLearn.Infras.External.SignalR;

namespace ZLearn.Infras.Realtime.AccessTracking
{
    public class AccessTrackingService : IAccessTrackingService
    {
        private static readonly ConcurrentDictionary<string, int> _accessCountStatus = new();
        private readonly IHubContext<AccessTrackingHub> _hubContext;

        public AccessTrackingService(
            IHubContext<AccessTrackingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task UpdateAccessCount()
        {
            await _hubContext.Clients.All.SendAsync("UpdateAccessCount", _accessCountStatus.Where(a => a.Value != 0).Count());
        }

        public async Task IncreaseAccess(string ip)
        {
            _accessCountStatus.AddOrUpdate(ip, 1, (key, oldValue) => oldValue + 1);
            await UpdateAccessCount();
        }

        public async Task DecreaseAccess(string ip)
        {
            if (_accessCountStatus.TryGetValue(ip, out var connectCount))
            {
                if (connectCount > 0)
                {
                    _accessCountStatus.TryUpdate(ip, connectCount - 1, connectCount);
                    await UpdateAccessCount();
                }
            }
        }

        public async Task<int> GetAccessCount(string ip)
        {
            return await Task.FromResult(_accessCountStatus.TryGetValue(ip, out var count) ? count : 0);
        }

        public async Task<int> GetAccessCount()
        {
            return await Task.FromResult(_accessCountStatus.Where(a => a.Value != 0).Count());
        }

        public Task<int> GetAccessCountToday()
        {
            return Task.FromResult(_accessCountStatus.Count);
        }
    }
}
