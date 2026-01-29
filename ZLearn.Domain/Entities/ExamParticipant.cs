using ZLearn.Domain.Common;
using ZLearn.Domain.Enums;

namespace ZLearn.Domain.Entities
{
    public class ExamParticipant : BaseEntity
    {
        public string? ParticipantCode { get; set; }
        public string ParticipantName { get; set; }
        public string UserId { get; set; }
        public Exam Exam { get; set; }
        public string ExamId { get; set; }
        public DateTimeOffset? FirstCheckIn { get; set; }
        public DateTimeOffset? LastCheckOut { get; set; }
        public ParticipantStatus Status { get; set; }
        public int Correct { get; set; }
        public int Completed { get; set; }
        public bool IsBanned { get; set; }
        public double Score { get; set; }
        public string SelectedAnswers { get; set; }
    }
}
