using System;

namespace Zlearn.V2.Domain.Common
{
    public abstract record DomainEvent : IDomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid OccurredOn { get; init; } = Guid.NewGuid();
        public abstract string AggregateId { get; }
    }
}
