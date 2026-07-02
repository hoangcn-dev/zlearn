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
        private readonly IMediator _mediator;

        public HandleEventsInterceptor(IMediator mediator)
        {
            _mediator = mediator;
        }

        private readonly List<OutboxEvent> _pendingPublish = new();

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateOutbox(eventData.Context);
            CollectPending(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateOutbox(eventData.Context);
            CollectPending(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            PublishPending(eventData.Context).GetAwaiter().GetResult();
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            await PublishPending(eventData.Context, cancellationToken);
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void CollectPending(DbContext? context)
        {
            if (context == null) return;
            var added = context.ChangeTracker.Entries<OutboxEvent>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToList();
            _pendingPublish.AddRange(added);
        }

        private async Task PublishPending(DbContext? context, CancellationToken cancellationToken = default)
        {
            if (!_pendingPublish.Any()) return;

            var eventsToPublish = _pendingPublish.ToList();
            _pendingPublish.Clear();

            foreach (var oe in eventsToPublish)
            {
                await _mediator.Publish(oe, cancellationToken);
            }
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
                Id = @event.EventId,
                OccurredOn = DateTimeOffset.UtcNow,
                Type = @event.GetType().AssemblyQualifiedName ?? @event.GetType().FullName ?? string.Empty,
                Content = JsonConvert.SerializeObject(@event)
            }).ToList();

            // 3. Lưu vào DbSet OutboxEvents
            context.Set<OutboxEvent>().AddRange(outboxEvents);

            // 4. Xóa các sự kiện đã xử lý để tránh lưu trùng lặp khi SaveChanges được gọi lại
            aggregates.ForEach(a => a.ClearUncommittedEvents());
        }
    }
}
