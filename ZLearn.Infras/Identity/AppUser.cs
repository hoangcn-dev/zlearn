using Microsoft.AspNetCore.Identity;
using ZLearn.Application.Common.Interfaces;

namespace ZLearn.Infras.Identity
{
    public class AppUser : IdentityUser<string>, IAppUser
    {
        public string? ImageUrl { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string NickName { get; set; }
        public bool IsShowNickName { get; set; }
        public DateTimeOffset LastLogin { get; set; }
        public bool IsActive { get; set; }
    }
}
