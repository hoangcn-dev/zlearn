using Microsoft.Extensions.Logging;
using ZLearn.Domain.Events.Cate;

namespace ZLearn.Application.Categories.EventHandlers
{
    public class CateCreatedEventHandler : INotificationHandler<CateCreatedEvent>
    {
        private readonly ILogger<CateCreatedEventHandler> _logger;

        public CateCreatedEventHandler(ILogger<CateCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(CateCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ZLearn Domain Event: {Event}", notification.GetType().Name);
            return Task.CompletedTask;
        }
    }
}
