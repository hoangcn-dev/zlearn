using System.Security.Claims;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Exams.DTOs;
namespace ZLearn.Application.Exams.Commands.JoinExam
{
    public class JoinExamCommandHandler : BaseCommandHandler, IRequestHandler<JoinExamCommand, ParticipantWaitingInfoDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IExamTrackingService _examTrackingService;

        public JoinExamCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo,
            IExamTrackingService examTrackingService) : base(mapper, mediator)
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
                Status = participantInfo.Status,
                ImageUrl = request.User.FindFirst("ImageUrl")!.Value,
                UserId = request.User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                CompletedCount = 0,
            };
            await _examTrackingService.AddParticipant(request.Data.ExamId, status);
            return participantInfo;
        }
    }
}
