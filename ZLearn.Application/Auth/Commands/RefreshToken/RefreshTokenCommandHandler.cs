using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.Identity;
using ZLearn.Application.Common.Identity.DTOs;

namespace ZLearn.Application.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : BaseCommandHandler, IRequestHandler<RefreshTokenCommand, JwtTokenDto>
    {
        private readonly IIdentityService _identityService;

        public RefreshTokenCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IIdentityService identityService) : base(mapper, mediator)
        {
            _identityService = identityService;
        }

        public async Task<JwtTokenDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var newToken = await _identityService.RefreshToken(request);
            return newToken;
        }
    }
}
