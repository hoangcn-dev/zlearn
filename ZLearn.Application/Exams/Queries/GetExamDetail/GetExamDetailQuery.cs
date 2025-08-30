using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Queries.GetExamDetail
{
    public class GetExamDetailQuery : IRequest<ExamDetailDto>
    {
        public string Id { get; set; }
    }
}
