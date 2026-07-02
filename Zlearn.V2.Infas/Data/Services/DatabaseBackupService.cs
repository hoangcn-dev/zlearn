using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Zlearn.V2.Application.Common.Utils;

namespace Zlearn.V2.Infas.Data.Services
{
    public class DatabaseBackupService : BackgroundService
    {
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly DatabasebackupConfiguration _config;
        private readonly string _backupPath;
        private Timer? _timer;
        private readonly string _connectionString;

        public DatabaseBackupService(
            IOptions<DatabasebackupConfiguration> options,
            ILogger<DatabaseBackupService> logger)
        {
            _config = options.Value;
            _logger = logger;
            _connectionString = EnvVariableHelper.GetValue(EnvVariableNames.CONNECTION_STRING_POSTGRES);
            _backupPath = Path.Combine(AppContext.BaseDirectory, _config.BackupDirectory);
            if (!Directory.Exists(_backupPath))
            {
                Directory.CreateDirectory(_backupPath);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_config.Enabled)
            {
                _logger.LogInformation("Database backup service is disabled");
                return Task.CompletedTask;
            }
            _logger.LogInformation("Database backup service is starting");
            _timer = new Timer(Backup, null, TimeSpan.Zero, TimeSpan.FromMinutes(_config.IntervalMinutes));
            return Task.CompletedTask;
        }

        private void Backup(object? state)
        {
            _logger.LogInformation("Starting database backup at {time}", DateTimeOffset.Now);
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(_connectionString);
                var database = builder.Database;
                var username = builder.Username;
                var password = builder.Password;
                var host = builder.Host;
                var port = builder.Port;

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"{_config.FilePrefix}_{database}_{timestamp}.sql";
                var backupFilePath = Path.Combine(_backupPath, backupFileName);

                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "pg_dump",
                        Arguments = $"-h {host} -p {port} -U {username} -F c -b -v -f \"{backupFilePath}\" {database}",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.StartInfo.EnvironmentVariables["PGPASSWORD"] = password;
                process.Start();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Database backup completed successfully to {file}", backupFilePath);
                    CleanupOldBackups();
                }
                else
                {
                    _logger.LogError("Database backup failed with exit code {code}: {error}",
                        process.ExitCode, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database backup");
            }
        }

        private void CleanupOldBackups()
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-_config.RetentionPeriodDays);
                var files = Directory.GetFiles(_backupPath, $"{_config.FilePrefix}*.sql");
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        fileInfo.Delete();
                        _logger.LogInformation("Deleted old backup file: {file}", fileInfo.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup of old backups");
            }
        }
    }

    public class DatabasebackupConfiguration
    {
        public string BackupDirectory { get; set; } = "Backups";
        public int RetentionPeriodDays { get; set; } = 7;
        public bool Enabled { get; set; }
        public int IntervalMinutes { get; set; } = 1440;
        public string FilePrefix { get; set; } = "backup";
    }
}
