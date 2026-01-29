using ZLearn.Application.Common.Queries;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Entities;
namespace ZLearn.Application.Exams.Queries.GetParticipantResult
{
    public class GetParticipantResultQueryHandler : BaseQueryHandler, IRequestHandler<GetParticipantResultQuery, ParticipantResultDto>
    {
        private readonly IExamRepo _examRepo;

        public GetParticipantResultQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }

        public async Task<ParticipantResultDto> Handle(GetParticipantResultQuery request, CancellationToken cancellationToken)
        {
            var res = await _examRepo.GetResult(request.ParticipantId, request.Alias);
            return res ?? throw new NotFoundException(nameof(Exam));
        }
    }
}
