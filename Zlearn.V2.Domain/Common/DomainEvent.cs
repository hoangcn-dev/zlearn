using System;

namespace Zlearn.V2.Domain.Common
{
    public abstract record DomainEvent : IDomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
        public abstract string AggregateId { get; }
    }
}
