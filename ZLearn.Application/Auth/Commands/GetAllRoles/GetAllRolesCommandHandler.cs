using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.Identity;

namespace ZLearn.Application.Auth.Commands.GetAllRoles
{
    public class GetAllRolesCommandHandler : BaseCommandHandler, IRequestHandler<GetAllRolesCommand, List<string>>
    {
        private readonly IIdentityService _identityService;

        public GetAllRolesCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IIdentityService identityService) : base(mapper, mediator)
        {
            _identityService = identityService;
        }

        public async Task<List<string>> Handle(GetAllRolesCommand request, CancellationToken cancellationToken)
        {
            var roles = await _identityService.GetAllSystemRoles();
            return roles;
        }
    }
}
