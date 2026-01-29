using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Queries.GetWaitExamInfo
{
    public class GetWaitExamInfoQuery : IRequest<WaitExamInfoDto>
    {
        public string ParticipantId { get; set; }
        public string Alias { get; set; }
    }
}
