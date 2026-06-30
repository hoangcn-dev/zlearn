using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;
using Newtonsoft.Json;
using ZLearn.Domain.Common;
using ZLearn.Infras.Data.Outbox;
using ZLearn.Application.Common.Interfaces;

namespace ZLearn.Infras.Data.Services
{
    public class OutboxProcessorJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessorJob> _logger;

        public OutboxProcessorJob(IServiceProvider serviceProvider, ILogger<OutboxProcessorJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox processor background service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxEventsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox events.");
                }

                // Chờ 2 giây trước khi quét tiếp
                await Task.Delay(2000, stoppingToken);
            }
        }

        private async Task ProcessOutboxEventsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = (DbContext)scope.ServiceProvider.GetRequiredService<IAppDbContext>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var outboxEvents = await dbContext.Set<OutboxEvent>()
                .Where(e => e.ProcessedOn == null)
                .OrderBy(e => e.OccurredOn)
                .Take(50)
                .ToListAsync(stoppingToken);

            if (!outboxEvents.Any()) return;

            _logger.LogInformation("Found {Count} unprocessed outbox events.", outboxEvents.Count);

            foreach (var outboxEvent in outboxEvents)
            {
                try
                {
                    var type = Type.GetType(outboxEvent.Type);
                    if (type == null)
                    {
                        throw new InvalidOperationException($"Could not load event type: {outboxEvent.Type}");
                    }

                    var domainEvent = JsonConvert.DeserializeObject(outboxEvent.Content, type);
                    if (domainEvent == null)
                    {
                        throw new InvalidOperationException($"Could not deserialize event of type {outboxEvent.Type}");
                    }

                    // Tạo wrapper động: DomainEventNotificationWrapper<TEvent>
                    var wrapperType = typeof(DomainEventNotificationWrapper<>).MakeGenericType(type);
                    var wrapper = Activator.CreateInstance(wrapperType, domainEvent);

                    // Publish qua MediatR
                    if (wrapper != null)
                    {
                        await mediator.Publish(wrapper, stoppingToken);
                    }

                    outboxEvent.ProcessedOn = DateTimeOffset.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox event with ID: {Id}", outboxEvent.Id);
                    outboxEvent.Error = ex.ToString();
                    // Vẫn đánh dấu hoặc bỏ qua tùy cơ chế retry, ở đây ta ghi lỗi để retry lần sau
                    // (processedOn vẫn là null nên sẽ được quét lại)
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}
