using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZLearn.Application.Files;

namespace ZLearn.Infras.Data.Services
{
    public class RemoveUnusedFilesService : BackgroundService
    {
        private readonly ILogger<RemoveUnusedFilesService> _logger;
        private readonly FileCleanupConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        public RemoveUnusedFilesService(
            IOptions<FileCleanupConfiguration> options,
            ILogger<RemoveUnusedFilesService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = options.Value;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_config.Enabled)
            {
                _logger.LogInformation("File cleanup service is disabled");
                return Task.CompletedTask;
            }
            _logger.LogInformation("File cleanup service is starting");
            var timer = new Timer(CleanupUnusedFiles, null, TimeSpan.Zero, TimeSpan.FromMinutes(_config.IntervalMinutes));
            return Task.CompletedTask;
        }

        private async void CleanupUnusedFiles(object? state)
        {
            _logger.LogInformation("Starting file cleanup at {time}", DateTimeOffset.Now);
            try
            {
                using var scrope = _serviceProvider.CreateScope();
                var fileRepo = scrope.ServiceProvider.GetRequiredService<IFileRepo>();
                var count = await fileRepo.Cleanup(TimeSpan.FromMinutes(_config.RemoveAfterMinutes));
                _logger.LogInformation("File cleanup completed. {count} files removed.", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file cleanup");
            }
        }
    }

    public class FileCleanupConfiguration
    {
        public bool Enabled { get; set; }
        public int IntervalMinutes { get; set; }
        public int RemoveAfterMinutes { get; set; }
    }
}
