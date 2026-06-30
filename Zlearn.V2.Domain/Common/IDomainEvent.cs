using System;

namespace Zlearn.V2.Domain.Common
{
    public interface IDomainEvent
    {
        public Guid EventId { get; init; }
    }
}
     