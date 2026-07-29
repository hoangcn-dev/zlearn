using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.IdentityContext.Users.Events
{
    public record UserLoggedOutEvent : DomainEvent
    {
        public string Id { get; set; }
        public DateTimeOffset LoggedOutAt { get; set; }
        public override string AggregateId => Id;
    }
}
