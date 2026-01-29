using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Queries.GetExamScore
{
    public class GetExamScoreQuery : IRequest<ExamScoreDto>
    {
        public string Id { get; set; }
        public string UserId { get; set; }
    }
}
