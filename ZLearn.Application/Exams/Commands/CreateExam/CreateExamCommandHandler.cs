using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Services;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Exams.Commands.ChangeExamStatus;
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

        public CreateExamCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo,
            IQuizRepo quizRepo,
            ISchedulerService schedulerService) : base(mapper, mediator)
        {
            _examRepo = examRepo;
            _quizRepo = quizRepo;
            _schedulerService = schedulerService;
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
                Status = ExamStatus.WaitStart,
                LockAccess = false,
                ShowAnswerAndKey = request.Data.ShowAnswerAndKey,
                MaxParticipants = request.Data.MaxParticipants,
                MixAnswers = request.Data.MixAnswers,
                MixQuestions = request.Data.MixQuestions,
                RequireJoinWithCode = request.Data.RequireJoinWithCode,
                RequireJoinWithName = request.Data.RequireJoinWithName,
            };

            var alias = StringHelper.GetRandomString(10);
            while (await _examRepo.Any(e => e.Alias == alias))
            {
                alias = StringHelper.GetRandomString(10);
            }
            exam.Alias = alias;
            _examRepo.Create(exam);
            await _examRepo.SaveChanges(); 

            var startExamCommand = new ChangeExamStatusCommand { ExamId = exam.Id, Status = ExamStatus.InProgress };
            if (request.Data.StartTime is null)
            {
                _schedulerService.ScheduleCommand(startExamCommand, exam.StartTime);
            }
            else
            {
                await _mediator.Send(startExamCommand, cancellationToken);
            }    

            return _mapper.Map<CreateResponseDto>(exam);
        }
    }
}
