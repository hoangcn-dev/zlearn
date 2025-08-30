using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.DTOs
{
    public class ExamDetailDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public string JoinUrl { get; set; }
        public string? JoinPass { get; set; }
        public bool LockAccess { get; set; }
        public bool ShowAnswerAndKey { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public long RemainingSecondsToEnd => EndTime == null ? -1 : (long)(EndTime.Value - DateTimeOffset.Now).TotalSeconds;
        public long RemainingSecondsToStart => (long)(StartTime - DateTimeOffset.Now).TotalSeconds;
        public ExamStatus Status { get; set; }
        public string QuizId { get; set; }
        public int MaxParticipants { get; set; }
    }
}
