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

namespace Zlearn.V2.Infas.Data.Services
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

                // Chờ 2 giây trước khi quét tiếp
                await Task.Delay(2000, stoppingToken);
            }
        }

        private async Task ProcessOutboxEventsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // 1. Lấy danh sách Top 10 TransactionId chưa xử lý và có RetryCount < 5
            var targetTxIds = await dbContext.OutboxEvents
                .Where(e => e.ProcessedOn == null && e.RetryCount < 5)
                .Select(e => e.TransactionId)
                .Distinct()
                .Take(10)
                .ToListAsync(stoppingToken);

            if (!targetTxIds.Any()) return;

            // 2. Kéo toàn bộ các OutboxEvent thuộc các TransactionId đó để đảm bảo tính toàn vẹn (chống phân mảnh giao dịch)
            var outboxEvents = await dbContext.OutboxEvents
                .Where(e => targetTxIds.Contains(e.TransactionId) && e.ProcessedOn == null && e.RetryCount < 5)
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
                        // Publish trực tiếp OutboxEvent qua MediatR để các Sync Handlers cập nhật Read Model (MongoDB)
                        await mediator.Publish(outboxEvent, stoppingToken);

                        outboxEvent.ProcessedOn = DateTimeOffset.UtcNow;
                        outboxEvent.Error = null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing V2 outbox event with ID: {Id}, TransactionId: {TxId}", outboxEvent.Id, outboxEvent.TransactionId);
                        outboxEvent.Error = ex.ToString();
                        outboxEvent.RetryCount += 1;
                    }
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}
