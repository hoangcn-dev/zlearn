using ZLearn.Application.Common.Commands;
using ZLearn.Application.Exams.DTOs;
namespace ZLearn.Application.Exams.Commands.JoinExam
{
    public class JoinExamCommandHandler : BaseCommandHandler, IRequestHandler<JoinExamCommand, ParticipantWaitingInfoDto>
    {
        private readonly IExamRepo _examRepo;

        public JoinExamCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }

        public async Task<ParticipantWaitingInfoDto> Handle(JoinExamCommand request, CancellationToken cancellationToken)
        {
            return await _examRepo.AddParticipant(request.User, request.Data);
        }
    }
}
