using ZLearn.Application.Common.Queries;
using ZLearn.Application.Realtime;
using ZLearn.Application.Tracking.DTOs;

namespace ZLearn.Application.Tracking.Queries.GetAccessCount
{
    public class GetAccessCountQueryHandler : BaseQueryHandler, IRequestHandler<GetAccessCountQuery, AccessCountStatDto>
    {
        private readonly IAccessHistoryRepo _accessHistoryRepo;
        private readonly IAccessTrackingService _accessTrackingService;

        public GetAccessCountQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IAccessHistoryRepo accessHistoryRepo,
            IAccessTrackingService accessTrackingService) : base(mapper, mediator)
        {
            _accessHistoryRepo = accessHistoryRepo;
            _accessTrackingService = accessTrackingService;
        }

        public async Task<AccessCountStatDto> Handle(GetAccessCountQuery request, CancellationToken cancellationToken)
        {
            var stat = new AccessCountStatDto
            {
                TotalInDay = await _accessTrackingService.GetAccessCountToday(),
                Total = await _accessHistoryRepo.GetTotalAccessCount()
            };
            return stat;
        }
    }
}
