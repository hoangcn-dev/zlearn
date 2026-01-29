using System.Security.Claims;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Exams.DTOs;
namespace ZLearn.Application.Exams.Queries.GetOnGoingExam
{
    public class GetOnGoingExamQueryHandler : BaseQueryHandler, IRequestHandler<GetOnGoingExamQuery, List<OnGoingExamListItemDto>>
    {
        private readonly IExamRepo _examRepo;

        public GetOnGoingExamQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }

        public async Task<List<OnGoingExamListItemDto>> Handle(GetOnGoingExamQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var exams = await _examRepo.GetOnGoingExams(userId);
            return exams;
        }
    }
}
