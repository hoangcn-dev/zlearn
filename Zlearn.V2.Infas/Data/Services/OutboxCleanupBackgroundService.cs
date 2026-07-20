using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zlearn.V2.Infas.Data.Outbox;

namespace Zlearn.V2.Infas.Data.Services
{
    public class OutboxCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxCleanupBackgroundService> _logger;

        public OutboxCleanupBackgroundService(IServiceProvider serviceProvider, ILogger<OutboxCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Cleanup Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupProcessedEventsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during Outbox Cleanup execution.");
                }

                // Chạy định kỳ mỗi 24 giờ một lần
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CleanupProcessedEventsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var retentionThreshold = DateTimeOffset.UtcNow.AddDays(-7);

            var oldEvents = await dbContext.OutboxEvents
                .Where(e => e.ProcessedOn != null && e.ProcessedOn < retentionThreshold)
                .ToListAsync(stoppingToken);

            if (oldEvents.Any())
            {
                _logger.LogInformation("Outbox Cleanup: Removing {Count} processed events older than 7 days.", oldEvents.Count);
                dbContext.OutboxEvents.RemoveRange(oldEvents);
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
