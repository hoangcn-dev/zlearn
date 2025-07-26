using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.Identity;

namespace ZLearn.Application.Auth.Queries.GetUserDetail
{
    public class GetUserDetailQueryHandler : BaseCommandHandler, IRequestHandler<GetUserDetailQuery, UserDetailDto>
    {
        private readonly IIdentityService _identityService;

        public GetUserDetailQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IIdentityService identityService) : base(mapper, mediator)
        {
            _identityService = identityService;
        }

        public async Task<UserDetailDto> Handle(GetUserDetailQuery request, CancellationToken cancellationToken)
        {
            var userDetail = await _identityService.GetUserDetail(request.Id)
                ?? throw new NotFoundException("AppUser", request.Id);
            return userDetail;
        }
    }
}
