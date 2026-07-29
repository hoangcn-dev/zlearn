using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Zlearn.V2.Application.Common.Commands;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Application.Quizzes;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Exams.Events;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Exams.Commands.ChangeExamStatus;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Common.Exceptions;

namespace Zlearn.V2.Application.Exams.Commands.CreateExam
{
    public class CreateExamCommandHandler : BaseCommandHandler, IRequestHandler<CreateExamCommand, CreateResponseDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IQuizWriteRepo _quizRepo;
        private readonly ISchedulerService _schedulerService;
        private readonly ILogger<CreateExamCommandHandler> _logger;

        public CreateExamCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo,
            IQuizWriteRepo quizRepo,
            ISchedulerService schedulerService,
            ILogger<CreateExamCommandHandler> logger) : base(mapper, mediator)
        {
            _examRepo = examRepo;
            _quizRepo = quizRepo;
            _schedulerService = schedulerService;
            _logger = logger;
        }

        public async Task<CreateResponseDto> Handle(CreateExamCommand request, CancellationToken cancellationToken)
        {
            if (await _examRepo.AnyAsync(e => e.Name == request.Data.Name))
                throw new ValidationErrorException("Tên bài kiểm tra đã tồn tại");
            if (!await _quizRepo.AnyAsync(q => q.Id == request.Data.QuizId))
                throw new ValidationErrorException("Đề kiểm tra không tồn tại");

            var current = DateTimeOffset.UtcNow;
            var alias = StringHelper.GetRandomString(10);
            while (await _examRepo.AnyAsync(e => e.Alias == alias))
            {
                alias = StringHelper.GetRandomString(10);
            }

            var exam = new Exam(
                request.Data.Name,
                alias,
                request.Data.QuizId,
                request.Data.JoinPass,
                request.Data.StartTime ?? current,
                request.Data.EndTime,
                request.Data.StartTime is null ? ExamStatus.InProgress : ExamStatus.WaitStart,
                request.Data.ShowAnswerAndKey,
                request.Data.MaxParticipants,
                request.Data.MixAnswers,
                request.Data.MixQuestions,
                request.Data.RequireJoinWithCode,
                request.Data.RequireJoinWithName,
                request.Data.AllowLateSubmit,
                request.Data.Note
            );

            // Schedule for starting and ending exam
            string? startJobId = null;
            if (exam.Status == ExamStatus.WaitStart)
            {
                var startExamCommand = new ChangeExamStatusCommand
                {
                    ExamId = exam.Id,
                    UserId = request.UserId,
                    Data = new ChangeExamStatusDto
                    {
                        Status = ExamStatus.InProgress,
                        LockAccess = exam.LockAccess
                    }
                };
                startJobId = await _schedulerService.ScheduleCommandExactly(startExamCommand, exam.Id, exam.StartTime);
            }

            string? endJobId = null;
            if (exam.EndTime is not null)
            {
                var endExamCommand = new ChangeExamStatusCommand
                {
                    ExamId = exam.Id,
                    Data = new ChangeExamStatusDto
                    {
                        Status = ExamStatus.Ended,
                        LockAccess = exam.LockAccess
                    },
                    UserId = request.UserId
                };
                endJobId = await _schedulerService.ScheduleCommandExactly(endExamCommand, exam.Id, exam.EndTime!.Value);
            }

            exam.UpdateJobIds(startJobId, endJobId);

            _examRepo.Create(exam);
            await _examRepo.SaveChangesAsync(cancellationToken);
            return _mapper.Map<CreateResponseDto>(exam);
        }
    }
}


