using System;
using Microsoft.AspNetCore.Identity;

namespace Zlearn.V2.Infas.Identity
{
    public class AppIdentityUser : IdentityUser<string>
    {
        public string? ImageUrl { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string NickName { get; set; } = string.Empty;
        public bool IsShowNickName { get; set; }
        public DateTimeOffset LastLogin { get; set; }
        public bool IsActive { get; set; }
    }
}
