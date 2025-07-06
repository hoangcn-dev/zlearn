using ZLearn.Application.Auth.DTOs;

namespace ZLearn.Application.Auth.Commands.SignIn
{
    public class SignInCommand : IRequest<UserSessionDataDto>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
