using ZLearn.Application.Common.Queries;
using ZLearn.Application.Exams.DTOs;
namespace ZLearn.Application.Exams.Queries.GetExamContent
{
    public class GetExamContentQueryHandler : BaseQueryHandler, IRequestHandler<GetExamContentQuery, ExamContentDto>
    {
        private readonly IExamRepo _examRepo;

        public GetExamContentQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }
        public async Task<ExamContentDto> Handle(GetExamContentQuery request, CancellationToken cancellationToken)
        {
            var exam = await _examRepo.GetExamContentAsync(request.Alias, request.UserId) 
                ?? throw new NotFoundException("Bài thi không tồn tại hoặc đã kết thúc.");
            return exam;
        }
    }
}
