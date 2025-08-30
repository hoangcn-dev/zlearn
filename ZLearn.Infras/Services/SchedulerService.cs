
using Hangfire;
using ZLearn.Application.Common.Services;

namespace ZLearn.Infras.Services
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public SchedulerService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public void CancelScheduledCommand(string jobId)
        {
            _backgroundJobClient.Delete(jobId);
        }

        public string ScheduleCommand<TCommand>(TCommand command, DateTimeOffset startAt) where TCommand : IRequest
        {
            return _backgroundJobClient.Schedule<IMediator>(
                mediator => mediator.Send(command, CancellationToken.None),
                startAt);
        }
    }
}
