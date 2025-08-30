using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.Queries.GetParticipantStatus
{
    public class GetParticipantStatusQuery : IRequest<ParticipantWaitingInfoDto?>
    {
        public string Alias { get; set; }
        public string UserId { get; set; }
    }
}
