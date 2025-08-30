using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.DTOs
{
    public class ExamStatusDto
    {
        public ExamStatus Status { get; set; }
        public int ParticipantsCount { get; set; }
        public bool IsLocked { get; set; }
    }
}
