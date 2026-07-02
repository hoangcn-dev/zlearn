using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Zlearn.V2.Infas.Data;
using Zlearn.V2.Infas.Data.Outbox;

namespace Zlearn.V2.Infas.Services.Projections
{
    public class OutboxFallbackHelper
    {
        private readonly AppDbContext _dbContext;
        private readonly IMediator _mediator;

        public OutboxFallbackHelper(AppDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task<bool> ProcessMissedEventsBeforeAsync(OutboxEvent currentEvent, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(currentEvent.AggregateId))
            {
                return false;
            }

            // 1. Tìm các event cùng AggregateId xảy ra trước đó và chưa được xử lý
            var missedEvents = await _dbContext.OutboxEvents
                .Where(e => e.AggregateId == currentEvent.AggregateId 
                            && e.ProcessedOn == null 
                            && e.OccurredOn < currentEvent.OccurredOn
                            && e.Id != currentEvent.Id)
                .OrderBy(e => e.OccurredOn)
                .ToListAsync(cancellationToken);

            if (!missedEvents.Any())
            {
                return false;
            }

            // 2. Xử lý lần lượt các event bị miss
            foreach (var missed in missedEvents)
            {
                // Gọi Mediator để dispatch event bị miss
                // Handler của missed event đó sẽ tự động chạy, cập nhật DB và đánh dấu ProcessedOn
                await _mediator.Publish(missed, cancellationToken);
            }

            return true;
        }
    }
}
