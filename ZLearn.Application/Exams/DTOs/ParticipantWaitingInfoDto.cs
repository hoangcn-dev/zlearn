using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.DTOs
{
    public class ParticipantWaitingInfoDto
    {
        public string ParticipantName { get; set; }
        public ParticipantStatus Status { get; set; }
        public string? ParticipantCode { get; set; }
        public long WaitTimeInSeconds { get; set; }
    }
}
