namespace ZLearn.Application.Logs.DTOs
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    public static class LogLevelExtensions
    {
        public static LogLevel FromString(string level)
        {
            return level.ToUpperInvariant() switch
            {
                "DBG" or "DEBUG" => LogLevel.Debug,
                "INF" or "INFO" or "INFORMATION" => LogLevel.Info,
                "WRN" or "WARN" or "WARNING" => LogLevel.Warning,
                "ERR" or "ERROR" => LogLevel.Error,
                "CRT" or "CRITICAL" or "FTL" or "FATAL" => LogLevel.Critical,
                _ => throw new ArgumentException($"Invalid log level: {level}", nameof(level))
            };
        }

        public static string ToLogString(this LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Debug => "DBG",
                LogLevel.Info => "INF",
                LogLevel.Warning => "WRN",
                LogLevel.Error => "ERR",
                LogLevel.Critical => "CRT",
                _ => "UNK"
            };
        }
    }
}
