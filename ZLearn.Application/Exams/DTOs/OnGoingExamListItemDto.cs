using System.Text.Json.Serialization;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.DTOs
{
    public class OnGoingExamListItemDto
    {
        public string Id { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ExamStatus Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string JoinUrl => "/bai-kiem-tra/join?alias=" + Alias;
    }
}
