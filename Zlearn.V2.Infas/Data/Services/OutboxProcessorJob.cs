using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;
using Newtonsoft.Json;
using Zlearn.V2.Infas.Data.Outbox;
using Zlearn.V2.Infas.Messaging;

namespace Zlearn.V2.Infas.Data.Services
{
    public class OutboxProcessorJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOutboxSignalChannel _signalChannel;
        private readonly ILogger<OutboxProcessorJob> _logger;

        public OutboxProcessorJob(IServiceProvider serviceProvider, IOutboxSignalChannel signalChannel, ILogger<OutboxProcessorJob> logger)
        {
            _serviceProvider = serviceProvider;
            _signalChannel = signalChannel;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("V2 Outbox processor background service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxEventsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing V2 outbox events.");
                }

                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(2));
                    await _signalChannel.WaitToReadAsync(cts.Token);
                }
                catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    // Polling 2 giây mặc định khi không có tín hiệu mới
                }
            }
        }

        private async Task ProcessOutboxEventsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // 1. Lấy danh sách Top 10 TransactionId chưa xử lý và có RetryCount < 5
            var targetTxIds = await dbContext.OutboxEvents
                .Where(e => e.ProcessedOn == null && !e.IsDeadLetter && e.RetryCount < 5)
                .Select(e => e.TransactionId)
                .Distinct()
                .Take(10)
                .ToListAsync(stoppingToken);

            if (!targetTxIds.Any()) return;

            // 2. Kéo toàn bộ các OutboxEvent thuộc các TransactionId đó để đảm bảo tính toàn vẹn (chống phân mảnh giao dịch)
            var outboxEvents = await dbContext.OutboxEvents
                .Where(e => targetTxIds.Contains(e.TransactionId) && e.ProcessedOn == null && !e.IsDeadLetter && e.RetryCount < 5)
                .OrderBy(e => e.OccurredOn)
                .ToListAsync(stoppingToken);

            if (!outboxEvents.Any()) return;

            _logger.LogInformation("V2: Found {Count} unprocessed outbox events across {TxCount} transactions.", outboxEvents.Count, targetTxIds.Count);

            var groupedByTx = outboxEvents.GroupBy(e => e.TransactionId);

            foreach (var group in groupedByTx)
            {
                foreach (var outboxEvent in group)
                {
                    try
                    {
                        var eventType = Type.GetType(outboxEvent.Type) 
                            ?? throw new InvalidOperationException($"Cannot resolve event type: '{outboxEvent.Type}'");

                        var domainEvent = JsonConvert.DeserializeObject(outboxEvent.Content, eventType) as INotification
                            ?? throw new InvalidOperationException($"Failed to deserialize outbox event content for type: '{outboxEvent.Type}'");

                        var publisher = scope.ServiceProvider.GetService<IRabbitMQPublisherService>();
                        if (publisher != null)
                        {
                            await publisher.PublishEventAsync(outboxEvent);
                        }
                        else
                        {
                            // Publish strongly-typed domain event via MediatR (fallback cho Unit Tests)
                            await mediator.Publish(domainEvent, stoppingToken);
                        }

                        outboxEvent.ProcessedOn = DateTimeOffset.UtcNow;
                        outboxEvent.Error = null;
                    }
                    catch (Exception ex)
                    {
                        outboxEvent.Error = ex.ToString();
                        outboxEvent.RetryCount += 1;

                        if (outboxEvent.RetryCount >= 5)
                        {
                            outboxEvent.IsDeadLetter = true;
                            _logger.LogCritical(ex, "ALERT: Outbox Event {Id} (Type: '{Type}', AggregateId: '{AggregateId}') failed after {RetryCount} retries and is moved to DEAD LETTER status.", outboxEvent.Id, outboxEvent.Type, outboxEvent.AggregateId, outboxEvent.RetryCount);
                        }
                        else
                        {
                            _logger.LogError(ex, "Error processing V2 outbox event with ID: {Id}, TransactionId: {TxId}, RetryCount: {RetryCount}", outboxEvent.Id, outboxEvent.TransactionId, outboxEvent.RetryCount);
                        }

                        // Dừng xử lý các event tiếp theo trong cùng Transaction để đảm bảo thứ tự (Causal Ordering)
                        break;
                    }
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}
