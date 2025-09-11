using Microsoft.Extensions.Logging;
using Quartz;
using ZLearn.Application.Common.Utils;

namespace ZLearn.Infras.External.Quartz.Jobs
{
    public class PublishCommandJob<TCommand> : IJob where TCommand : IRequest
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PublishCommandJob<TCommand>> _logger;

        public const string COMMAND_OBJECT_KEY = nameof(COMMAND_OBJECT_KEY);

        public PublishCommandJob(IMediator mediator, ILogger<PublishCommandJob<TCommand>> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var stringData = context.MergedJobDataMap.GetString(COMMAND_OBJECT_KEY);
            var command = StringHelper.JsonStringToObject<TCommand>(stringData!);
            if (command == null)
            {
                _logger.LogError($"PublishCommandJob<{nameof(TCommand)}>: Command data is null or invalid");
                return;
            }
            _logger.LogInformation($"PublishCommandJob<{nameof(TCommand)}>: Publishing command");
            await _mediator.Send(command);
        }
    }
}
