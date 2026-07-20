using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zlearn.V2.Domain.Common;
using Zlearn.V2.Infas.Data.Outbox;

namespace Zlearn.V2.Infas.Data.Interceptors
{
    public class HandleEventsInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateOutbox(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateOutbox(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateOutbox(DbContext? context)
        {
            if (context == null) return;

            // 1. Quét các thực thể kế thừa AggregateRoot có uncommitted events
            var aggregates = context.ChangeTracker.Entries<AggregateRoot>()
                .Select(e => e.Entity)
                .Where(e => e.UncommittedEvents.Any())
                .ToList();

            var domainEvents = aggregates.SelectMany(a => a.UncommittedEvents).ToList();

            if (!domainEvents.Any()) return;

            var transactionId = context.Database.CurrentTransaction?.TransactionId ?? Guid.NewGuid();

            // 2. Chuyển đổi sang OutboxEvent
            var outboxEvents = domainEvents.Select(@event => new OutboxEvent
            {
                Id = @event.EventId,
                TransactionId = transactionId,
                OccurredOn = DateTimeOffset.UtcNow,
                Type = @event.GetType().AssemblyQualifiedName ?? @event.GetType().FullName ?? string.Empty,
                Content = JsonConvert.SerializeObject(@event),
                AggregateId = @event.AggregateId,
                RetryCount = 0
            }).ToList();

            // 3. Lưu vào DbSet OutboxEvents
            context.Set<OutboxEvent>().AddRange(outboxEvents);

            // 4. Xóa các sự kiện đã xử lý để tránh lưu trùng lặp khi SaveChanges được gọi lại
            aggregates.ForEach(a => a.ClearUncommittedEvents());
        }
    }
}
