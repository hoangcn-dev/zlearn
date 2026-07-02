using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using ZLearn.Domain.Common;
using ZLearn.Infras.Data.Outbox;

namespace ZLearn.Infras.Data.Interceptors
{
    public class OutboxInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateOutbox(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateOutbox(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
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

            // 2. Chuyển đổi sang OutboxEvent
            var outboxEvents = domainEvents.Select(@event => new OutboxEvent
            {
                Id = Guid.NewGuid(),
                OccurredOn = DateTimeOffset.UtcNow,
                Type = @event.GetType().AssemblyQualifiedName ?? @event.GetType().FullName ?? string.Empty,
                Content = JsonConvert.SerializeObject(@event)
            }).ToList();

            // 3. Lưu vào DbSet OutboxEvents
            context.Set<OutboxEvent>().AddRange(outboxEvents);

            // 4. Xóa các sự kiện đã được lưu trên Aggregate
            aggregates.ForEach(a => a.ClearUncommittedEvents());
        }
    }
}
