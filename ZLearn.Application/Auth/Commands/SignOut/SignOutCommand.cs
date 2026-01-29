namespace ZLearn.Application.Auth.Commands.SignOut
{
    public class SignOutCommand : IRequest
    {
        public string AccessToken { get; set; }
    }
}
