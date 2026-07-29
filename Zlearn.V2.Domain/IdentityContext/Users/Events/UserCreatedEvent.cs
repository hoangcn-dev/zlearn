using Zlearn.V2.Domain.CatalogContext.Tags;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.IdentityContext.Users.Events
{
    public record UserCreatedEvent : DomainEvent
    {
        public override string AggregateId => Id;
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string NickName { get; set; }
        public bool IsShowNickName { get; set; }
        public DateTimeOffset LastLogin { get; set; }
        public bool IsActive { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
