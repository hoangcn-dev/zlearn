using Hangfire;
using Microsoft.Extensions.Logging;
using Quartz;
using ZLearn.Application.Common.Services;
using ZLearn.Application.Common.Utils;
using ZLearn.Infras.External.Quartz.Jobs;

namespace ZLearn.Infras.Services.Scheduler
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ISchedulerFactory _schedulerFactory;

        public SchedulerService(
            IBackgroundJobClient backgroundJobClient, 
            ISchedulerFactory schedulerFactory)
        {
            _backgroundJobClient = backgroundJobClient;
            _schedulerFactory = schedulerFactory;
        }

        public async Task<bool> CancelExactlyScheduleById(string jobId, string groupName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            return await scheduler.DeleteJob(new JobKey(jobId, groupName));
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

        public async Task<string> ScheduleCommandExactly<TCommand>(TCommand command, string groupName, DateTimeOffset startAt) where TCommand : IRequest
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobId = IdGenerator.Generate("JOB");
            var job = JobBuilder.Create<PublishCommandJob<TCommand>>()
                .WithIdentity(jobId, groupName)
                .UsingJobData(PublishCommandJob<TCommand>.COMMAND_OBJECT_KEY, StringHelper.ObjectToJsonString(command))
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity(jobId, groupName)
                .ForJob(job)
                .StartAt(startAt)
                .Build();
            await scheduler.ScheduleJob(job, trigger);
            return jobId;
        }

        public async Task<string> SchedulePublishEventExactly<TEvent>(TEvent @event, string groupName, DateTimeOffset startAt) where TEvent : INotification
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobId = IdGenerator.Generate("JOB");
            var job = JobBuilder.Create<PublishEventJob<TEvent>>()
                .WithIdentity(jobId, groupName)
                .UsingJobData(PublishEventJob<TEvent>.EVENT_OBJECT_KEY, StringHelper.ObjectToJsonString(@event))
                .Build();
            var trigger = TriggerBuilder.Create()
                .WithIdentity(jobId, groupName)
                .ForJob(job)
                .StartAt(startAt)
                .Build();
            await scheduler.ScheduleJob(job, trigger);
            return jobId;
        }

        public string ScheduleRecurringCommand<TCommand>(TCommand comand, DateTimeOffset? endAt, TimeSpan delay) where TCommand : IRequest
        {
            var jobId = IdGenerator.Generate("RCT");
            RecurringJob.AddOrUpdate<IMediator>(
                jobId, 
                mediator => mediator.Send(comand, CancellationToken.None), 
                $"*/{delay.TotalMinutes} * * * *");

            if (endAt.HasValue)
            {
                _backgroundJobClient.Schedule(() => RecurringJob.RemoveIfExists(jobId), enqueueAt: endAt!.Value);
            }

            return jobId;
        }

        public string ScheduleRecurringEvent<TEvent>(TEvent @event, DateTimeOffset? endAt, TimeSpan delay) where TEvent : INotification
        {
            var jobId = IdGenerator.Generate("RCT");
            RecurringJob.AddOrUpdate<IMediator>(
                jobId,
                mediator => mediator.Publish(@event, CancellationToken.None),
                $"*/{delay.TotalMinutes} * * * *");

            if (endAt.HasValue)
            {
                _backgroundJobClient.Schedule(() => RecurringJob.RemoveIfExists(jobId), enqueueAt: endAt!.Value);
            }

            return jobId;
        }
    }
}
