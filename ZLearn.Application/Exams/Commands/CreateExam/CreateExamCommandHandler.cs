using Microsoft.Extensions.Logging;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Services;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Exams.Commands.ChangeExamStatus;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Application.Quizzes;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;
using ZLearn.Domain.Exceptions;
namespace ZLearn.Application.Exams.Commands.CreateExam
{
    public class CreateExamCommandHandler : BaseCommandHandler, IRequestHandler<CreateExamCommand, CreateResponseDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IQuizRepo _quizRepo;
        private readonly ISchedulerService _schedulerService;
        private readonly ILogger<CreateExamCommandHandler> _logger;

        public CreateExamCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo,
            IQuizRepo quizRepo,
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
            if (await _examRepo.Any(e => e.Name == request.Data.Name))
                throw new ValidationErrorException("Tên bài kiểm tra đã tồn tại");
            if (!await _quizRepo.Any(q => q.Id == request.Data.QuizId))
                throw new ValidationErrorException("Đề kiểm tra không tồn tại");
            var current = DateTimeOffset.UtcNow;
            var exam = new Exam
            {
                Id = IdGenerator.Generate("EXA"),
                Name = request.Data.Name,
                QuizId = request.Data.QuizId,
                JoinPass = request.Data.JoinPass,
                StartTime = request.Data.StartTime ?? current,
                EndTime = request.Data.EndTime,
                Status = request.Data.StartTime is null? ExamStatus.InProgress : ExamStatus.WaitStart,
                LockAccess = false,
                ShowAnswerAndKey = request.Data.ShowAnswerAndKey,
                MaxParticipants = request.Data.MaxParticipants,
                MixAnswers = request.Data.MixAnswers,
                MixQuestions = request.Data.MixQuestions,
                RequireJoinWithCode = request.Data.RequireJoinWithCode,
                RequireJoinWithName = request.Data.RequireJoinWithName,
                AllowLateSubmit = request.Data.AllowLateSubmit,
            };

            var alias = StringHelper.GetRandomString(10);
            while (await _examRepo.Any(e => e.Alias == alias))
            {
                alias = StringHelper.GetRandomString(10);
            }
            exam.Alias = alias;

            // Schedule for starting and ending exam
            if (exam.Status == ExamStatus.WaitStart)
            {
                var startExamCommand = new ChangeExamStatusCommand { 
                    ExamId = exam.Id, 
                    UserId = request.UserId,
                    Data = new ChangeExamStatusDto
                    {
                        Status = ExamStatus.InProgress,
                        LockAccess = exam.LockAccess
                    }
                };
                exam.StartJobId = await _schedulerService.ScheduleCommandExactly(startExamCommand, exam.Id, exam.StartTime);
            }
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
                exam.EndJobId = await _schedulerService.ScheduleCommandExactly(endExamCommand, exam.Id, exam.EndTime!.Value);
            }

            _examRepo.Create(exam);
            await _examRepo.SaveChanges();
            return _mapper.Map<CreateResponseDto>(exam);
        }
    }
}
