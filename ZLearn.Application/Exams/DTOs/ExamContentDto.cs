using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Exams.DTOs
{
    public class ExamContentDto
    {
        public string Id { get; set; }
        public string ExamName { get; set; }
        public string Alias { get; set; }
        public string ParticipantName { get; set; }
        public string? ParticipantCode { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public long RemainingSecondsToEnd => EndTime == null ? -1 : (long) (EndTime.Value - DateTimeOffset.Now).TotalSeconds;
        public List<QuestionContentDto> Questions { get; set; }
    }
}
