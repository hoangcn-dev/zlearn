using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Domain.Events.Cate;

namespace ZLearn.Application.Categories.EventHandlers
{
    public class CateUpdatedEventHander : INotificationHandler<CateUpdatedEvent>
    {
        private readonly ILogger<CateUpdatedEventHander> _logger;

        public CateUpdatedEventHander(ILogger<CateUpdatedEventHander> logger)
        {
            _logger = logger;
        }

        public Task Handle(CateUpdatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ZLearn Domain Event: {Event}", notification.GetType().Name);
            return Task.CompletedTask;
        }
    }
}
