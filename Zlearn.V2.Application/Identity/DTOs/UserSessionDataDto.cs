
namespace Zlearn.V2.Application.Identity.DTOs
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
