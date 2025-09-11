using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.DTOs
{
    public class ChangeExamStatusDto
    {
        public ExamStatus Status { get; set; }
        public bool LockAccess { get; set; }
    }
}
