using System.Linq.Expressions;
using System.Windows.Input;

namespace ZLearn.Application.Common.Services
{
    public interface ISchedulerService
    {
        string ScheduleCommand<TCommand>(TCommand command, DateTimeOffset startAt) where TCommand : IRequest;
        Task<string> SchedulePublishEventExactly<TEvent>(TEvent @event, string groupName, DateTimeOffset startAt) where TEvent : INotification;
        Task<string> ScheduleCommandExactly<TCommand>(TCommand command, string groupName, DateTimeOffset startAt) where TCommand : IRequest;
        Task<bool> CancelExactlyScheduleById(string jobId, string groupName);
        void CancelScheduledCommand(string jobId);
        string ScheduleRecurringCommand<TCommand>(TCommand command, DateTimeOffset? endAt, TimeSpan delay) where TCommand : IRequest;
        string ScheduleRecurringEvent<TEvent>(TEvent @event, DateTimeOffset? endAt, TimeSpan delay) where TEvent : INotification;
    }
}
