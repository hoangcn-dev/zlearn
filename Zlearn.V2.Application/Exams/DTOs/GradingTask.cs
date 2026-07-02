using System.Collections.Generic;

namespace Zlearn.V2.Application.Exams.DTOs
{
    public class GradingTask
    {
        public string UserId { get; set; }
        public string ExamId { get; set; }
        public List<SubmitAnswerDto> Answers { get; set; }
        public int Seq { get; set; }
    }
}



