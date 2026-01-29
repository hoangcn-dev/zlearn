using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.Identity;

namespace ZLearn.Application.Auth.Queries.GetAllRoles
{
    public class GetAllRolesQueryHandler : BaseCommandHandler, IRequestHandler<GetAllRolesQuery, List<string>>
    {
        private readonly IIdentityService _identityService;

        public GetAllRolesQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IIdentityService identityService) : base(mapper, mediator)
        {
            _identityService = identityService;
        }

        public async Task<List<string>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _identityService.GetAllSystemRoles();
            return roles;
        }
    }
}
