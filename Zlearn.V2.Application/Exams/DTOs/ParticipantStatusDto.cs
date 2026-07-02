using System.Text.Json.Serialization;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams.DTOs
{
    public class ParticipantStatusDto
    {
        public string ParticipantId { get; set; }
        public string UserId { get; set; }
        public string ImageUrl { get; set; }
        public string? ParticipantCode { get; set; }
        public string ParticipantName { get; set; }
        public int CompletedCount { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ParticipantStatus Status { get; set; }
        public DateTimeOffset Timestamp => DateTimeOffset.UtcNow;
    }
}



