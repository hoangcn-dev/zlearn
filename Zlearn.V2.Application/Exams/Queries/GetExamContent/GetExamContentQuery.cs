using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams.Queries.GetExamContent
{
    public class GetExamContentQuery : IRequest<ExamContentDto>
    {
        public string UserId { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
    }

    public class GetExamContentQueryHandler : IRequestHandler<GetExamContentQuery, ExamContentDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IExamTrackingService _examTrackingService;

        public GetExamContentQueryHandler(
            IExamRepo examRepo,
            IExamTrackingService examTrackingService)
        {
            _examRepo = examRepo;
            _examTrackingService = examTrackingService;
        }

        public async Task<ExamContentDto> Handle(GetExamContentQuery request, CancellationToken cancellationToken)
        {
            var result = await _examRepo.GetExamContentAsync(request.Alias, request.UserId)
                ?? throw new NotFoundException("Bài thi không tồn tại, chưa bắt đầu hoặc đã kết thúc.");

            var (exam, ep) = result;
            await _examTrackingService.UpdateParticipantStatus(exam.Id, ep.UserId, (ParticipantStatus)ep.Status);
            return exam;
        }
    }
}


