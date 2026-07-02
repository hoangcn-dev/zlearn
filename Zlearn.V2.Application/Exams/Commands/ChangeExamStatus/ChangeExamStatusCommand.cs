using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Domain.ExamContext.Exams;

namespace Zlearn.V2.Application.Exams.Commands.ChangeExamStatus
{
    public class ChangeExamStatusCommand : IRequest
    {
        public string ExamId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public ChangeExamStatusDto Data { get; set; } = null!;
    }

    public class ChangeExamStatusCommandHandler : IRequestHandler<ChangeExamStatusCommand>
    {
        private readonly IExamRepo _examRepo;
        private readonly IExamTrackingService _examTrackingService;
        private readonly ILogger<ChangeExamStatusCommandHandler> _logger;
        private readonly ISchedulerService _schedulerService;

        public ChangeExamStatusCommandHandler(
            IExamRepo examRepo,
            IExamTrackingService examTrackingService,
            ILogger<ChangeExamStatusCommandHandler> logger,
            ISchedulerService schedulerService)
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

            var exam = await _examRepo.Get(e => e.Id.ToLower() == request.ExamId.ToLower() && e.CreatedBy == request.UserId)
                ?? throw new ForbiddenException();

            exam.ChangeStatus((Zlearn.V2.Domain.ExamContext.Exams.ExamStatus)request.Data.Status);
            exam.SetLockAccess(request.Data.LockAccess);

            if (exam.Status == Zlearn.V2.Domain.ExamContext.Exams.ExamStatus.InProgress)
            {
                if (exam.StartJobId is not null && await _schedulerService.CancelExactlyScheduleById(exam.StartJobId, exam.Id))
                {
                    exam.UpdateJobIds(null, exam.EndJobId);
                    _logger.LogInformation($"Cancelled start job {exam.StartJobId} for exam {exam.Id} at {exam.StartTime.ToLocalTime().ToString("HH:mm:ss")}");
                }
                exam.Start(DateTimeOffset.UtcNow);
            }

            _examRepo.Update(exam);
            await _examRepo.SaveChangesAsync(cancellationToken);

            await _examTrackingService.UpdateExamStatus(request.ExamId, request.Data.Status);
            _logger.LogInformation($"Exam {request.ExamId} status changed to {request.Data.Status}");
        }
    }
}


