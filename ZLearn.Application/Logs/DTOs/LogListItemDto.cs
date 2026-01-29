using System.Text.Json.Serialization;

namespace ZLearn.Application.Logs.DTOs
{
    public class LogListItemDto
    {
        public DateTimeOffset Timestamp { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string? Exception { get; set; }
        public string? UserId { get; set; }
        public string? IpAddress { get; set; }
    }
}
