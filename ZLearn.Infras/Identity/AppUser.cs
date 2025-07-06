using Microsoft.AspNetCore.Identity;
using ZLearn.Application.Common.Interfaces;

namespace ZLearn.Infras.Identity
{
    public class AppUser : IdentityUser<string>, IAppUser
    {
        public string ImagePath { get; set; }
    }
}
