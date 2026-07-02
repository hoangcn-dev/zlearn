using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Exams;

namespace Zlearn.V2.Application.Exams.Commands.ManageParticipant
{
    public class ManageParticipantCommand : IRequest<string>
    {
        public ManageParticipantDto Data { get; set; } = null!;
        public string ExamId { get; set; } = string.Empty;
        public ClaimsPrincipal UserClaims { get; set; } = null!;
    }

    public class ManageParticipantCommandHandler : IRequestHandler<ManageParticipantCommand, string>
    {
        private readonly IExamRepo _examRepo;

        public ManageParticipantCommandHandler(IExamRepo examRepo)
        {
            _examRepo = examRepo;
        }

        public async Task<string> Handle(ManageParticipantCommand request, CancellationToken cancellationToken)
        {
            var userId = request.UserClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (!await _examRepo.IsExamCreator(request.ExamId, userId))
                throw new ForbiddenException();

            return await _examRepo.ManageParticipant(request.ExamId, request.Data.ParticipantId, request.Data.Action);
        }
    }
}


