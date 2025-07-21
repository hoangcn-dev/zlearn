using ZLearn.Application.Common.Identity.DTOs;
using ZLearn.Application.Common.Interfaces;

namespace ZLearn.Application.Auth.DTOs
{
    public class UserSessionDataDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public JwtTokenDto Token { get; set; }
        public IList<string> Roles { get; set; }
    }
}
