using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Queries.GetParticipantResult
{
    public class GetParticipantResultQuery : IRequest<ParticipantResultDto>
    {
        public string ParticipantId { get; set; }
        public string Alias { get; set; }
    }
}
