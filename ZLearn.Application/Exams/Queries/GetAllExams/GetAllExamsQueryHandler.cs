using ZLearn.Application.Common.Queries;
using ZLearn.Application.Exams.DTOs;
namespace ZLearn.Application.Exams.Queries.GetAllExams
{
    public class GetAllExamsQueryHandler : BaseQueryHandler, IRequestHandler<GetAllExamsQuery, List<ExamListItemDto>>
    {
        private readonly IExamRepo _examRepo;

        public GetAllExamsQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }
        public async Task<List<ExamListItemDto>> Handle(GetAllExamsQuery request, CancellationToken cancellationToken)
        {
            var exams = await _examRepo.GetAll(
                projector: e => new ExamListItemDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Status = e.Status
                },
                orderBy: e => e.CreatedAt,
                isAsc: false);
            return exams;
        }
    }
}
