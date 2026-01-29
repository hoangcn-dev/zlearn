using Microsoft.Extensions.Configuration;
using ZLearn.Application.Logs;
using ZLearn.Application.Logs.DTOs;

namespace ZLearn.Infras.Services.Log
{
    public class LogService : ILogService
    {
        private readonly IConfiguration _configuration;

        public LogService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<LogListItemDto>> GetLogsOfDay(DateTime? date)
        {
            date ??= DateTime.Now;
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configuration["Serilog:WriteTo:0:Args:path"]!);
            string filePath = $"{defaultPath}{date:yyyyMMdd}";

            var logs = new List<LogListItemDto>();
            if (!File.Exists(filePath)) return logs;

            // Read the log file
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            var lines = new List<string>();
            string? logLine;
            while ((logLine = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(logLine)) continue;
                lines.Add(logLine);
            }

            LogListItemDto? current = null;
            var regex = new System.Text.RegularExpressions.Regex(
                @"^(?<timestamp>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) \[(?<level>\w+)\] (?<message>.*) \[UserId:(?<userid>.*?)\] \[Ip:(?<ip>.*?)\]$");
            foreach (var logEntry in lines)
            {
                var match = regex.Match(logEntry);
                if (match.Success)
                {
                    if (current != null) logs.Add(current);
                    current = new LogListItemDto
                    {
                        Timestamp = DateTime.Parse(match.Groups["timestamp"].Value),
                        Level = LogLevelExtensions.FromString(match.Groups["level"].Value),
                        Message = match.Groups["message"].Value,
                        Exception = null,
                        UserId = match.Groups["userid"].Value,
                        IpAddress = match.Groups["ip"].Value
                    };
                }
                else if (current != null)
                {
                    if (string.IsNullOrEmpty(current.Exception))
                        current.Exception = logEntry;
                    else
                        current.Exception += Environment.NewLine + logEntry;
                }
            }
            if (current != null) logs.Add(current);
            logs.Sort((x, y) => y.Timestamp.CompareTo(x.Timestamp)); // Sort by time
            return logs;
        }
    }
}
