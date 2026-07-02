using System;

namespace ZLearn.Domain.Common
{
    public interface IDomainEvent
    {
        DateTimeOffset OccurredOn { get; }
    }
}
