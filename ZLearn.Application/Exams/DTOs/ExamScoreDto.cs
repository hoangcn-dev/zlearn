using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.DTOs
{
    public class ExamScoreDto
    {
        public string ExamId { get; set; }
        public string ExamName { get; set; }
        public int MaxParticipants { get; set; }
        public int QuestionCount { get; set; }
        public List<ExamParticipantScoreDto> ParticipantScores { get; set; } = new List<ExamParticipantScoreDto>();
    }

    public class ExamParticipantScoreDto
    {
        public string ParticipantId { get; set; }
        public string UserId { get; set; }
        public string ParticipantName { get; set; }
        public string? ParticipantCode { get; set; }
        public long Duration { get; set; }
        public int Completed { get; set; }
        public double Score { get; set; }
        public int Correct { get; set; }
        public ParticipantStatus Status { get; set; }
        public DateTimeOffset FirstCheckIn { get; set; }
        public DateTimeOffset LastCheckOut { get; set; }
    }
}
