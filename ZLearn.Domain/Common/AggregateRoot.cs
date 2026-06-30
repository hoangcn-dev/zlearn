using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZLearn.Domain.Common
{
    public abstract class AggregateRoot : AuditableEntity
    {
        public int Version { get; protected set; } = 1;

        private readonly List<IDomainEvent> _uncommittedEvents = new();

        [NotMapped]
        public IReadOnlyCollection<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

        public void RaiseEvent(IDomainEvent @event)
        {
            _uncommittedEvents.Add(@event);
        }

        public void ClearUncommittedEvents()
        {
            _uncommittedEvents.Clear();
        }
    }
}
