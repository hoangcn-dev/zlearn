using Microsoft.Extensions.Logging;
using ZLearn.Domain.Events.Cate;

namespace ZLearn.Application.Categories.EventHandlers
{
    public class CateDeletedEventHandler : INotificationHandler<CateDeletedEvent>
    {
        private readonly ILogger<CateDeletedEventHandler> _logger;

        public CateDeletedEventHandler(ILogger<CateDeletedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CateDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ZLearn Domain Event: {Event}", notification.GetType().Name);
            return Task.CompletedTask;
        }
    }
}
