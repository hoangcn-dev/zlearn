using System.Windows.Input;

namespace ZLearn.Application.Common.Services
{
    public interface ISchedulerService
    {
        string ScheduleCommand<TCommand>(TCommand command, DateTimeOffset startAt) where TCommand : IRequest;
        void CancelScheduledCommand(string jobId);
    }
}
