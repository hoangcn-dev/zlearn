using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Identity;

namespace ZLearn.Application.Auth.Commands.UpdateUserProfile
{
    public class UpdateUserProfileCommandHandler : BaseCommandHandler, IRequestHandler<UpdateUserProfileCommand, UpdateResponseDto>
    {
        private readonly IIdentityService _identityService;

        public UpdateUserProfileCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IIdentityService identityService) : base(mapper, mediator)
        {
            _identityService = identityService;
        }

        public async Task<UpdateResponseDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.UpdateUserProfile(request);
            return result;
        }
    }
}
