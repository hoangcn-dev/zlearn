using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Queries.GetAllExams
{
    public class GetAllExamsQuery : IRequest<List<ExamListItemDto>>
    {
        public string UserId { get; set; }
    }
}
