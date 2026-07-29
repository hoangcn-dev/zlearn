using System;
using MediatR;

namespace Zlearn.V2.Infas.Data.Outbox
{
    public class OutboxEvent : INotification
    {
        public Guid Id { get; set; }
        public Guid? TransactionId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset OccurredOn { get; set; }
        public DateTimeOffset? ProcessedOn { get; set; }
        public string? Error { get; set; }
        public int RetryCount { get; set; }
        public bool IsDeadLetter { get; set; }
        public string AggregateId { get; set; } = string.Empty;
    }
}
