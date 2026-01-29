using Microsoft.Extensions.Logging;
using ZLearn.Application.Common.Events;

namespace ZLearn.Application.Exams.Events.ExamChangeStatus
{
    public class ExamChangeStatusEventHandler : BaseEventHandler, INotificationHandler<ExamChangeStatusEvent>
    {
        private readonly IExamTrackingService _examTrackingService;

        public ExamChangeStatusEventHandler(
            IMapper mapper,
            IMediator mediator,
            ILogger<ExamChangeStatusEventHandler> logger,
            IExamTrackingService examTrackingService) : base(mapper, mediator, logger)
        {
            _examTrackingService = examTrackingService;
        }

        public async Task Handle(ExamChangeStatusEvent notification, CancellationToken cancellationToken)
        {
            await _examTrackingService.UpdateExamStatus(notification.ExamId, notification.Status);
            _logger.LogInformation($"Exam {notification.ExamId} status changed to {notification.Status}");
        }
    }
}
