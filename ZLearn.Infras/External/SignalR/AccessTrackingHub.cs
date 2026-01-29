using Microsoft.AspNetCore.SignalR;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Realtime;

namespace ZLearn.Infras.External.SignalR
{
    public class AccessTrackingHub : Hub
    {
        private readonly IAccessTrackingService _accessTrackingService;

        public AccessTrackingHub(IAccessTrackingService accessTrackingService)
        {
            _accessTrackingService = accessTrackingService;
        }

        public override async Task OnConnectedAsync()
        {
            string ip = Context.GetHttpContext()?.GetIPAddress() ?? "unknown";
            await _accessTrackingService.IncreaseAccess(ip);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string ip = Context.GetHttpContext()?.GetIPAddress() ?? "unknown";
            await _accessTrackingService.DecreaseAccess(ip);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
