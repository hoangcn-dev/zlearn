using ZLearn.Application.Common.Queries;
using ZLearn.Application.Logs.DTOs;

namespace ZLearn.Application.Logs.Queries
{
    public class GetLogQueryHandler : BaseQueryHandler, IRequestHandler<GetLogQuery, List<LogListItemDto>>
    {
        private readonly ILogService _logService;

        public GetLogQueryHandler(
            IMapper mapper,
            IMediator mediator,
            ILogService logService) : base(mapper, mediator)
        {
            _logService = logService;
        }

        public async Task<List<LogListItemDto>> Handle(GetLogQuery request, CancellationToken cancellationToken)
        {
            var logs = await _logService.GetLogsOfDay(request.Date);
            return logs;
        }
    }
}
