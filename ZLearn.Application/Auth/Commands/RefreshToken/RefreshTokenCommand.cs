using ZLearn.Application.Common.Identity.DTOs;

namespace ZLearn.Application.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<JwtTokenDto>
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
