using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.Commands.ChangeExamStatus
{
    public class ChangeExamStatusCommand : IRequest
    {
        public string ExamId { get; set; }
        public string UserId { get; set; }
        public ChangeExamStatusDto Data { get; set; }
    }
}
