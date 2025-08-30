using ZLearn.Application.Common.Queries;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Entities;
namespace ZLearn.Application.Exams.Queries.GetExamStatus
{
    public class GetExamStatusQueryHandler : BaseQueryHandler, IRequestHandler<GetExamStatusQuery, ExamStatusDto>
    {
        private readonly IExamRepo _examRepo;

        public GetExamStatusQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }

        public async Task<ExamStatusDto> Handle(GetExamStatusQuery request, CancellationToken cancellationToken)
        {
            var status = await _examRepo.Get(
                filter: e => e.Alias == request.Alias,
                projector: e => new ExamStatusDto
                {
                    Status = e.Status,
                    IsLocked = e.LockAccess,
                });
            return status ?? throw new NotFoundException(nameof(Exam));
        }
    }
}
