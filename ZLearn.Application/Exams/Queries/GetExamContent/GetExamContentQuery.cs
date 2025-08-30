using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Queries.GetExamContent
{
    public class GetExamContentQuery : IRequest<ExamContentDto>
    {
        public string UserId { get; set; }
        public string Alias { get; set; }
    }
}
