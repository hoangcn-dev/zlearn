using Microsoft.Extensions.Logging;
using Quartz;
using ZLearn.Application.Common.Utils;

namespace ZLearn.Infras.External.Quartz.Jobs
{
    public class PublishEventJob<TEvent> : IJob where TEvent : INotification
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PublishEventJob<TEvent>> _logger;

        public const string EVENT_OBJECT_KEY = nameof(EVENT_OBJECT_KEY);

        public PublishEventJob(IMediator mediator, ILogger<PublishEventJob<TEvent>> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var stringData = context.MergedJobDataMap.GetString(EVENT_OBJECT_KEY);
            var @event = StringHelper.JsonStringToObject<TEvent>(stringData!);
            if (@event == null)
            {
                _logger.LogError($"PublishEventJob<{nameof(TEvent)}>: Event data is null or invalid");
                return;
            }
            _logger.LogInformation($"PublishEventJob<{nameof(TEvent)}>: Publishing event");
            await _mediator.Publish(@event);
        }
    }
}
