using ZLearn.Application.Common.Commands;
namespace ZLearn.Application.Exams.Commands.SubmitAnswer
{
    public class SubmitAnswerCommandHandler : BaseCommandHandler, IRequestHandler<SubmitAnswerCommand, string>
    {
        private readonly IExamRepo _examRepo;

        public SubmitAnswerCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }
        public async Task<string> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            await _examRepo.SaveResult(request.ParticipantId, request.Data);
            return request.Data.ExamId;
        }
    }
}
