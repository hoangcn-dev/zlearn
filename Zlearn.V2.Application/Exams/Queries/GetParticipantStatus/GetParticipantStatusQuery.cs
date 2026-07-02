using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Exams;

namespace Zlearn.V2.Application.Exams.Queries.GetParticipantStatus
{
    public class GetParticipantStatusQuery : IRequest<ParticipantWaitingInfoDto?>
    {
        public string Alias { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class GetParticipantStatusQueryHandler : IRequestHandler<GetParticipantStatusQuery, ParticipantWaitingInfoDto?>
    {
        private readonly IExamRepo _examRepo;

        public GetParticipantStatusQueryHandler(IExamRepo examRepo)
        {
            _examRepo = examRepo;
        }

        public async Task<ParticipantWaitingInfoDto?> Handle(GetParticipantStatusQuery request, CancellationToken cancellationToken)
        {
            return await _examRepo.GetParticipantStatusAsync(request.UserId, request.Alias);
        }
    }
}


