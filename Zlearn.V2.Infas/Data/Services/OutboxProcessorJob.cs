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

            var outboxEvents = await dbContext.OutboxEvents
                .Where(e => e.ProcessedOn == null)
                .OrderBy(e => e.OccurredOn)
                .Take(50)
                .ToListAsync(stoppingToken);

            if (!outboxEvents.Any()) return;

            _logger.LogInformation("V2: Found {Count} unprocessed outbox events.", outboxEvents.Count);

            foreach (var outboxEvent in outboxEvents)
            {
                try
                {
                    // Publish trực tiếp OutboxEvent qua MediatR để các handlers xử lý
                    await mediator.Publish(outboxEvent, stoppingToken);

                    // Nếu các handler chưa gán ProcessedOn (do không có handler tương ứng), tự động gán
                    if (outboxEvent.ProcessedOn == null)
                    {
                        outboxEvent.ProcessedOn = DateTimeOffset.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing V2 outbox event with ID: {Id}", outboxEvent.Id);
                    outboxEvent.Error = ex.ToString();
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}
