
using Microsoft.Extensions.Logging;

namespace ZLearn.Application.Exams.Services
{
    public class SendRemainingTimeTaskHandler : IRequestHandler<SendRemainingTimeTask>
    {
        private readonly IExamTrackingService _examTrackingService;
        private readonly ILogger<SendRemainingTimeTaskHandler> _logger;

        public SendRemainingTimeTaskHandler(IExamTrackingService examTrackingService, ILogger<SendRemainingTimeTaskHandler> logger)
        {
            _examTrackingService = examTrackingService;
            _logger = logger;
        }

        public async Task Handle(SendRemainingTimeTask request, CancellationToken cancellationToken)
        {
            var current = DateTimeOffset.UtcNow;
            if (request.StartTime > current || request.TimeStamp <= current) return;
            var remainingTime = request.TimeStamp - DateTimeOffset.UtcNow;
            await _examTrackingService.UpdateRemainingTime(
                request.ExamId, (long) remainingTime.TotalMilliseconds, request.HubMethodName);
            _logger.LogInformation($"Sent {remainingTime.TotalMilliseconds} ms {request.HubMethodName}");
        }
    }
}
