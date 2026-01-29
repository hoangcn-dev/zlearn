using ZLearn.Domain.Enums;

namespace ZLearn.Application.Exams.Events.ExamChangeStatus
{
    [Serializable]
    public class ExamChangeStatusEvent : INotification
    {
        public string ExamId { get; set; }
        public string UserId { get; set; }
        public ExamStatus Status { get; set; }
    }
}
