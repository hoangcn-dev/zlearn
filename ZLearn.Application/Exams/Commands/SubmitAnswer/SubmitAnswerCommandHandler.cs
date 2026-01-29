using ZLearn.Application.Common.Commands;
using ZLearn.Domain.Enums;
namespace ZLearn.Application.Exams.Commands.SubmitAnswer
{
    public class SubmitAnswerCommandHandler : BaseCommandHandler, IRequestHandler<SubmitAnswerCommand, string>
    {
        private readonly IExamRepo _examRepo;
        private readonly IExamTrackingService _examTrackingService;

        public SubmitAnswerCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo,
            IExamTrackingService examTrackingService) : base(mapper, mediator)
        {
            _examRepo = examRepo;
            _examTrackingService = examTrackingService;
        }

        public async Task<string> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            await _examRepo.SaveResult(request.ParticipantId, request.Data);
            await _examTrackingService.UpdateParticipantStatus(request.Data.ExamId, request.ParticipantId, ParticipantStatus.Completed);
            return request.Data.ExamId;
        }
    }
}
