using System.Security.Claims;
using ZLearn.Application.Auth.DTOs;

namespace ZLearn.Application.Auth.Commands.GetAuthData
{
    public class GetAuthDataQuery : IRequest<UserSessionDataDto>
    {
        public ClaimsPrincipal Claims { get; set; }
    }
}
