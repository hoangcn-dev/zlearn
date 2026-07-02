using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Identity.Commands;
using Zlearn.V2.Application.Identity.DTOs;
using Zlearn.V2.Application.Identity.Queries;

namespace Zlearn.V2.Application.Identity
{
    // COMMAND HANDLERS
    public class SignInCommandHandler : IRequestHandler<SignInCommand, UserSessionDataDto>
    {
        private readonly IIdentityService _identityService;

        public SignInCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<UserSessionDataDto> Handle(SignInCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.AuthenticateAsync(request);
        }
    }

    public class GoogleSignInCommandHandler : IRequestHandler<GoogleSignInCommand, UserSessionDataDto?>
    {
        private readonly IIdentityService _identityService;

        public GoogleSignInCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<UserSessionDataDto?> Handle(GoogleSignInCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.AuthenticateWithGoogle(request.AuthenticateResult);
        }
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, JwtTokenDto>
    {
        private readonly IIdentityService _identityService;

        public RefreshTokenCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<JwtTokenDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.RefreshToken(request);
        }
    }

    public class SignOutCommandHandler : IRequestHandler<SignOutCommand>
    {
        private readonly IIdentityService _identityService;

        public SignOutCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task Handle(SignOutCommand request, CancellationToken cancellationToken)
        {
            await _identityService.EndSessionAsync(request);
        }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateResponseDto>
    {
        private readonly IIdentityService _identityService;

        public UpdateUserCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<UpdateResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.UpdateUser(request);
        }
    }

    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UpdateResponseDto>
    {
        private readonly IIdentityService _identityService;

        public UpdateUserProfileCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<UpdateResponseDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.UpdateUserProfile(request);
        }
    }

    // QUERY HANDLERS
    public class GetAuthDataQueryHandler : IRequestHandler<GetAuthDataQuery, UserSessionDataDto>
    {
        public async Task<UserSessionDataDto> Handle(GetAuthDataQuery request, CancellationToken cancellationToken)
        {
            var sessionData = new UserSessionDataDto
            {
                Id = request.Claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                UserName = request.Claims.FindFirst("UserName")?.Value ?? string.Empty,
                LastName = request.Claims.FindFirst("LastName")?.Value ?? string.Empty,
                FirstName = request.Claims.FindFirst("FirstName")?.Value ?? string.Empty,
                ImagePath = request.Claims.FindFirst("ImageUrl")?.Value ?? "https://res.cloudinary.com/hoangcn-dev/image/upload/v1700000000/default-avatar.png",
                Roles = request.Claims.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            };
            return await Task.FromResult(sessionData);
        }
    }

    public class GetListUsersQueryHandler : IRequestHandler<GetListUsersQuery, PaginatedDto<UserListItemDto>>
    {
        private readonly IIdentityService _identityService;

        public GetListUsersQueryHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<PaginatedDto<UserListItemDto>> Handle(GetListUsersQuery request, CancellationToken cancellationToken)
        {
            return await _identityService.GetAllUsers(request);
        }
    }

    public class GetUserDetailQueryHandler : IRequestHandler<GetUserDetailQuery, UserDetailDto>
    {
        private readonly IIdentityService _identityService;

        public GetUserDetailQueryHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<UserDetailDto> Handle(GetUserDetailQuery request, CancellationToken cancellationToken)
        {
            return await _identityService.GetUserDetail(request.Id);
        }
    }

    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<string>>
    {
        private readonly IIdentityService _identityService;

        public GetAllRolesQueryHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<List<string>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            return await _identityService.GetAllSystemRoles();
        }
    }
}
