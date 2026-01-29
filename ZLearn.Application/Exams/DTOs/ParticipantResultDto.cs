namespace ZLearn.Application.Exams.DTOs
{
    public class ParticipantResultDto
    {
        public int ParticipantsCount { get; set; }
        public int QuestionsCount { get; set; }
        public int CorrectCount { get; set; }
        public int CompletedCount { get; set; }
        public int Rank { get; set; }
        public DateTimeOffset FirstCheckIn { get; set; }
        public DateTimeOffset LastCheckOut { get; set; }
        public double Score { get; set; }
    }
}
