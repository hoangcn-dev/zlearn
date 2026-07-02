using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.IdentityContext.Users
{
    public class AppUser : AuditableEntity
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string NickName { get; set; } = string.Empty;
        public bool IsShowNickName { get; set; }
        public DateTimeOffset LastLogin { get; set; }
        public bool IsActive { get; set; }
    }
}
