using ZLearn.Application.Common.Commands;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Enums;
namespace ZLearn.Application.Exams.Queries.GetParticipantStatus
{
    public class GetParticipantStatusQueryHandler : BaseCommandHandler, IRequestHandler<GetParticipantStatusQuery, ParticipantWaitingInfoDto?>
    {
        private readonly IExamRepo _examRepo;

        public GetParticipantStatusQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }

        public async Task<ParticipantWaitingInfoDto?> Handle(GetParticipantStatusQuery request, CancellationToken cancellationToken)
        {
            return await _examRepo.GetParticipantStatusAsync(request.UserId, request.Alias);
        }
    }
}
