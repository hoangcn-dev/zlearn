using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.IdentityContext.Roles.Events
{
    public record RoleCreatedEvent : DomainEvent
    {
        public override string AggregateId => Id;
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NormalizedName { get; set; }
    }
}
