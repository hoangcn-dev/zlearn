using System;
using MediatR;

namespace Zlearn.V2.Domain.Common
{
    public interface IDomainEvent : INotification
    {
        public Guid EventId { get; init; }
        public DateTimeOffset OccurredOn { get; }
        public string AggregateId { get; }
    }
}
     