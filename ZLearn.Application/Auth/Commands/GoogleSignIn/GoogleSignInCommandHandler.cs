using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.Identity;

namespace ZLearn.Application.Auth.Commands.GoogleSignIn
{
    public class GoogleSignInCommandHandler : BaseCommandHandler, IRequestHandler<GoogleSignInCommand, UserSessionDataDto?>
    {
        private readonly IIdentityService _identityService;

        public GoogleSignInCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IIdentityService identityService) : base(mapper, mediator)
        {
            _identityService = identityService;
        }

        public async Task<UserSessionDataDto?> Handle(GoogleSignInCommand request, CancellationToken cancellationToken)
        {
            var sessionData = await _identityService.AuthenticateWithGoogle(request.AuthenticateResult);
            return sessionData;
        }
    }
}
