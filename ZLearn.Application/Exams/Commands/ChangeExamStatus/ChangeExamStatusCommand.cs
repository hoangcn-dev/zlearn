using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.Commands.ChangeExamStatus
{
    public class ChangeExamStatusCommand : IRequest
    {
        public string ExamId { get; set; }
        public ExamStatus Status { get; set; }
    }
}
