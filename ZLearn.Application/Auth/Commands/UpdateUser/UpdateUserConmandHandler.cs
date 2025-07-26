using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Identity;

namespace ZLearn.Application.Auth.Commands.UpdateUser
{
    public class UpdateUserConmandHandler : BaseCommandHandler, IRequestHandler<UpdateUserConmand, UpdateResponseDto>
    {
        private readonly IIdentityService _identityService;

        public UpdateUserConmandHandler(
            IMapper mapper,
            IMediator mediator,
            IIdentityService identityService) : base(mapper, mediator)
        {
            _identityService = identityService;
        }

        public async Task<UpdateResponseDto> Handle(UpdateUserConmand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.UpdateUser(request);
            return result;
        }
    }
}
