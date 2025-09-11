using Microsoft.Extensions.Logging;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.Services;
using ZLearn.Domain.Enums;
namespace ZLearn.Application.Exams.Commands.ChangeExamStatus
{
    public class ChangeExamStatusCommandHandler : BaseCommandHandler, IRequestHandler<ChangeExamStatusCommand>
    {
        private readonly IExamRepo _examRepo;
        private readonly IExamTrackingService _examTrackingService;
        private readonly ILogger<ChangeExamStatusCommandHandler> _logger;
        private readonly ISchedulerService _schedulerService;

        public ChangeExamStatusCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo,
            IExamTrackingService examTrackingService,
            ILogger<ChangeExamStatusCommandHandler> logger,
            ISchedulerService schedulerService) : base(mapper, mediator)
        {
            _examRepo = examRepo;
            _examTrackingService = examTrackingService;
            _logger = logger;
            _schedulerService = schedulerService;
        }

        public async Task Handle(ChangeExamStatusCommand request, CancellationToken cancellationToken)
        {
            if (request.Data.Status == ExamStatus.Ended)
            { 
                await _examRepo.EndExam(request.UserId, request.ExamId);
                return;
            }

            var exam = await _examRepo.Get(filter: e => e.Id.ToLower() == request.ExamId.ToLower() && e.CreatedBy == request.UserId)
                ?? throw new ForbiddenException();
            exam.Status = request.Data.Status;
            exam.LockAccess = request.Data.LockAccess;
            if (exam.Status == ExamStatus.InProgress)
            {
                if (exam.StartJobId is not null && await _schedulerService.CancelExactlyScheduleById(exam.StartJobId, exam.Id))
                {
                    exam.StartJobId = null;
                    _logger.LogInformation($"Cancelled start job {exam.StartJobId} for exam {exam.Id} at {exam.StartTime.ToLocalTime().ToString("HH:mm:ss")}");
                }
                exam.StartTime = DateTimeOffset.UtcNow;
            }
            _examRepo.Update(exam);
            await _examRepo.SaveChanges();

            await _examTrackingService.UpdateExamStatus(request.ExamId, request.Data.Status);
            _logger.LogInformation($"Exam {request.ExamId} status changed to {request.Data.Status}");
        }
    }
}
