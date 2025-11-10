using ZLearn.Application.Common.Queries;
using ZLearn.Application.Realtime;
using ZLearn.Application.Tracking.DTOs;

namespace ZLearn.Application.Tracking.Queries.GetAccessCount
{
    public class GetAccessCountQueryHandler : BaseQueryHandler, IRequestHandler<GetAccessCountQuery, AccessCountStatDto>
    {
        private readonly IAccessHistoryRepo _accessHistoryRepo;

        public GetAccessCountQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IAccessHistoryRepo accessHistoryRepo) : base(mapper, mediator)
        {
            _accessHistoryRepo = accessHistoryRepo;
        }

        public async Task<AccessCountStatDto> Handle(GetAccessCountQuery request, CancellationToken cancellationToken)
        {
            var stat = new AccessCountStatDto
            {
                TotalInDay = await _accessHistoryRepo.GetAccessCountThisMonth(),
                Total = await _accessHistoryRepo.GetTotalAccessCount()
            };
            return stat;
        }
    }
}
