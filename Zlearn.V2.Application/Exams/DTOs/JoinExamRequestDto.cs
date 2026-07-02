namespace Zlearn.V2.Application.Exams.DTOs
{
    public class JoinExamRequestDto
    {
        public string ExamId { get; set; }
        public string? ParticipantName { get; set; }
        public string? ParticipantCode { get; set; }
        public string? Password { get; set; }
    }
}



