using MediatR;
using ZLearn.Domain.Common;

namespace ZLearn.Infras.Data.Outbox
{
    public class DomainEventNotificationWrapper<TEvent> : INotification where TEvent : IDomainEvent
    {
        public TEvent DomainEvent { get; }

        public DomainEventNotificationWrapper(TEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}
