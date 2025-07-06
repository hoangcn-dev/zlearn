using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.Identity;

namespace ZLearn.Application.Auth.Commands.SignIn
{
    public class SignInCommandHandler : IRequestHandler<SignInCommand, UserSessionDataDto>
    {
        private readonly IIdentityService _identityService;

        public SignInCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<UserSessionDataDto> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            var sessionData = await _identityService.AuthenticateAsync(request);
            return sessionData;
        }
    }
}
