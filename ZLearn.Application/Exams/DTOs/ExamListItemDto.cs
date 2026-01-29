using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.DTOs
{
    public class ExamListItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public ExamStatus Status { get; set; }
    }
}
