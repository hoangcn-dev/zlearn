using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Queries.GetExamStatus
{
    public class GetExamStatusQuery : IRequest<ExamStatusDto>
    {
        public string Alias { get; set; }
    }
}
