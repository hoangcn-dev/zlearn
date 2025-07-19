using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.Identity;

namespace ZLearn.Application.Auth.Commands.SignOut
{
    public class SignOutCommandHandler : BaseCommandHandler, IRequestHandler<SignOutCommand>
    {
        private readonly IIdentityService _identityService;

        public SignOutCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IIdentityService identityService) : base(mapper, mediator)
        {
            _identityService = identityService;
        }

        public async Task Handle(SignOutCommand request, CancellationToken cancellationToken)
        {
            await _identityService.EndSessionAsync(request);
        }
    }
}
