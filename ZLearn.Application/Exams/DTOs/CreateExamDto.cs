namespace ZLearn.Application.Exams.DTOs
{
    public class CreateExamDto
    {
        public string Name { get; set; } = string.Empty;
        public string? JoinPass { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public bool MixQuestions { get; set; }
        public bool MixAnswers { get; set; }
        public string QuizId { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public bool ShowAnswerAndKey { get; set; }
        public bool RequireJoinWithCode { get; set; }
        public bool RequireJoinWithName { get; set; }
        public bool AllowLateSubmit { get; set; }
        public string? Note { get; set; }
    }
}
