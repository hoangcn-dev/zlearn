using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.IdentityContext.Users.Events
{
    public record UserUpdatedEvent : DomainEvent
    {
        public override string AggregateId => Id;
        public string Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string NickName { get; set; }
        public bool IsShowNickName { get; set; }
        public bool IsActive { get; set; }
    }
}
