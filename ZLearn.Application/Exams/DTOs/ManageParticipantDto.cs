using System.Security.Claims;
using System.Text.Json.Serialization;

namespace ZLearn.Application.Exams.DTOs
{
    public class ManageParticipantDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ManageParticipantAction Action { get; set; }
        public string ParticipantId { get; set; }
    }

    public enum ManageParticipantAction
    {
        Remove,
        Block,
        Unblock
    }
}
