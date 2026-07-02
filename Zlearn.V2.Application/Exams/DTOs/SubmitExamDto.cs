namespace Zlearn.V2.Application.Exams.DTOs
{
    public class SubmitExamDto
    {
        public string ExamId { get; set; }
        public List<SubmitAnswerDto> Answers { get; set; }
    }

    public class SubmitAnswerDto
    {
        public string QuestionId { get; set; }
        public List<int> SubmitKeys { get; set; } = new();
    }
}



