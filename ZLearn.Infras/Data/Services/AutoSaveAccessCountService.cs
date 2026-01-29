using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Realtime;
using ZLearn.Domain.Entities;
using ZLearn.Infras.Services.AccessTracking;

namespace ZLearn.Infras.Data.Services
{
    public class AutoSaveAccessCountService : BackgroundService
    {
        private readonly ILogger<AutoSaveAccessCountService> _logger;
        private readonly AccessTrackingConfig _config;
        private Timer? _timer;
        private readonly IAccessTrackingService _accessTrackingService;
        private readonly IServiceProvider _serviceProvider;


        public AutoSaveAccessCountService(
            ILogger<AutoSaveAccessCountService> logger,
            IOptions<AccessTrackingConfig> options,
            IAccessTrackingService accessTrackingService,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = options.Value;
            _accessTrackingService = accessTrackingService;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_config.Enabled)
            {
                _logger.LogInformation("Auto save access history service is disabled");
                return Task.CompletedTask;
            }
            _logger.LogInformation("Auto save access history service is starting");
            _timer = new Timer(AutoSaveAccessCount, null, TimeSpan.Zero, TimeSpan.FromMinutes(_config.AutoSaveIntervalMinutes));
            return Task.CompletedTask;
        }

        private async void AutoSaveAccessCount(object? state)
        {
            using var scope = _serviceProvider.CreateScope();
            var accessHistoryRepo = scope.ServiceProvider.GetRequiredService<IAccessHistoryRepo>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            try
            {
                var count = await _accessTrackingService.GetAccessCountToday();
                var his = await accessHistoryRepo.Get(h => h.Day == today);
                if (his is null)
                {
                    his = new AccessHistory
                    {
                        Id = IdGenerator.Generate("AHI"),
                        Day = today,
                        AccessCount = count
                    };
                    accessHistoryRepo.Create(his);
                    await accessHistoryRepo.SaveChanges();
                }
                else
                {
                    his.AccessCount = count;
                    accessHistoryRepo.Update(his);
                    await accessHistoryRepo.SaveChanges();
                }
                _logger.LogInformation("Auto-saved access count successfully at {time}", DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-saving access count at {time}", DateTimeOffset.Now);
            }
        }
    }
}
