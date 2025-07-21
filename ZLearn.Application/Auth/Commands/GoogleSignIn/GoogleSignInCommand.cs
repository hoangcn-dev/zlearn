using Microsoft.AspNetCore.Authentication;
using ZLearn.Application.Auth.DTOs;

namespace ZLearn.Application.Auth.Commands.GoogleSignIn
{
    public class GoogleSignInCommand : IRequest<UserSessionDataDto?>
    {
        public AuthenticateResult? AuthenticateResult { get; set; }
    }
}
