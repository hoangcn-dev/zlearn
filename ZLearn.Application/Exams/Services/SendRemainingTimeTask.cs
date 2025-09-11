namespace ZLearn.Application.Exams.Services
{
    public class SendRemainingTimeTask : IRequest
    {
        public DateTimeOffset TimeStamp { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public string ExamId { get; set; }
        public string HubMethodName { get; set; }
    }
}
