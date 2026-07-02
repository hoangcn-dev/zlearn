using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams.DTOs
{
    public class ExamStatusDto
    {
        public ExamStatus Status { get; set; }
        public bool LockAccess { get; set; }
    }
}



