using ZLearn.Application.Common.Queries;
using ZLearn.Application.Exams.DTOs;
namespace ZLearn.Application.Exams.Queries.GetExamContent
{
    public class GetExamContentQueryHandler : BaseQueryHandler, IRequestHandler<GetExamContentQuery, ExamContentDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IExamTrackingService _examTrackingService;

        public GetExamContentQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo,
            IExamTrackingService examTrackingService) : base(mapper, mediator)
        {
            _examRepo = examRepo;
            _examTrackingService = examTrackingService;
        }
        public async Task<ExamContentDto> Handle(GetExamContentQuery request, CancellationToken cancellationToken)
        {
            var (exam, ep) = await _examRepo.GetExamContentAsync(request.Alias, request.UserId) 
                ?? throw new NotFoundException("Bài thi không tồn tại, chưa bắt đầu hoặc đã kết thúc.");
            await _examTrackingService.UpdateParticipantStatus(exam.Id, ep.UserId, ep.Status);
            return exam;
        }
    }
}
