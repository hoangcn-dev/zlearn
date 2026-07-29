using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.IdentityContext.Users.Events
{
    public record UserSignedInEvent : DomainEvent
    {
        public string Id { get; set; }
        public DateTimeOffset SignedInAt { get; set; }
        public override string AggregateId => Id;
    }
}
