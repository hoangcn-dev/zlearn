using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zlearn.V2.Domain.Common
{
    public abstract class AggregateRoot : AuditableEntity
    {
        public int Version { get; protected set; } = 1;

        private readonly List<DomainEvent> _uncommittedEvents = [];

        [NotMapped]
        public IReadOnlyCollection<DomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

        public void RaiseEvent(DomainEvent @event)
        {
            _uncommittedEvents.Add(@event);
        }

        public void ClearUncommittedEvents()
        {
            _uncommittedEvents.Clear();
        }
    }
}
