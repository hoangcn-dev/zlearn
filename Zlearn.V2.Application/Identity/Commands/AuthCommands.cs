using MediatR;
using Microsoft.AspNetCore.Authentication;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Identity.DTOs;

namespace Zlearn.V2.Application.Identity.Commands
{
    public class SignInCommand : IRequest<UserSessionDataDto>
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class GoogleSignInCommand : IRequest<UserSessionDataDto?>
    {
        public AuthenticateResult? AuthenticateResult { get; set; }
    }

    public class RefreshTokenCommand : IRequest<JwtTokenDto>
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class SignOutCommand : IRequest
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    public class UpdateUserCommand : IRequest<UpdateResponseDto>
    {
        public string Id { get; set; } = string.Empty;
        public UserUpdateContentDto UpdateData { get; set; } = new();
    }

    public class UpdateUserProfileCommand : IRequest<UpdateResponseDto>
    {
        public string Id { get; set; } = string.Empty;
        public UpdateUserProfileDto UpdateData { get; set; } = new();
    }
}
