using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Domain.ExamContext.Exams;

namespace Zlearn.V2.Application.Exams.Queries.GetParticipantResult
{
    public class GetParticipantResultQuery : IRequest<ParticipantResultDto>
    {
        public string ParticipantId { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
    }

    public class GetParticipantResultQueryHandler : IRequestHandler<GetParticipantResultQuery, ParticipantResultDto>
    {
        private readonly IExamRepo _examRepo;

        public GetParticipantResultQueryHandler(IExamRepo examRepo)
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


