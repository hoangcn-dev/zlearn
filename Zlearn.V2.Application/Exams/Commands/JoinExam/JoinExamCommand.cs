using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams.Commands.JoinExam
{
    public class JoinExamCommand : IRequest<ParticipantWaitingInfoDto>
    {
        public ClaimsPrincipal User { get; set; } = null!;
        public JoinExamRequestDto Data { get; set; } = null!;
    }

    public class JoinExamCommandHandler : IRequestHandler<JoinExamCommand, ParticipantWaitingInfoDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IExamTrackingService _examTrackingService;

        public JoinExamCommandHandler(
            IExamRepo examRepo,
            IExamTrackingService examTrackingService)
        {
            _examRepo = examRepo;
            _examTrackingService = examTrackingService;
        }

        public async Task<ParticipantWaitingInfoDto> Handle(JoinExamCommand request, CancellationToken cancellationToken)
        {
            var participantInfo = await _examRepo.AddParticipant(request.User, request.Data);
            var status = new ParticipantStatusDto
            {
                ParticipantId = participantInfo.ParticipantId,
                ParticipantName = participantInfo.ParticipantName,
                ParticipantCode = participantInfo.ParticipantCode,
                Status = (ParticipantStatus)participantInfo.Status,
                ImageUrl = request.User.FindFirst("ImageUrl")?.Value ?? string.Empty,
                UserId = request.User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                CompletedCount = 0,
            };
            await _examTrackingService.AddParticipant(request.Data.ExamId, status);
            return participantInfo;
        }
    }
}


