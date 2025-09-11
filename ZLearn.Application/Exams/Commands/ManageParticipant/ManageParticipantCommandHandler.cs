using System.Security.Claims;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Enums;
namespace ZLearn.Application.Exams.Commands.ManageParticipant
{
    public class ManageParticipantCommandHandler : BaseCommandHandler, IRequestHandler<ManageParticipantCommand, string>
    {
        private readonly IExamRepo _examRepo;

        public ManageParticipantCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }

        public async Task<string> Handle(ManageParticipantCommand request, CancellationToken cancellationToken)
        {
            if (!await _examRepo.IsExamCreator(request.ExamId, request.UserClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value))
                throw new ForbiddenException();
            return await _examRepo.ManageParticipant(request.ExamId, request.Data.ParticipantId, request.Data.Action);
        }
    }
}
