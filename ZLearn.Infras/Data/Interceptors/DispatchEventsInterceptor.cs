using ZLearn.Domain.Common;

namespace ZLearn.Infras.Data.Interceptors
{
    public class DispatchEventsInterceptor : SaveChangesInterceptor
    {
        private readonly IMediator _mediator;

        public DispatchEventsInterceptor(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            DispatchEvents(eventData.Context).GetAwaiter().GetResult();
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            await DispatchEvents(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public async Task DispatchEvents(DbContext? context)
        {
            if (context == null) return;

            // Get all entity have the events
            var entites = context.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.Entity.Events.Any())
                .Select(e => e.Entity)
                .ToList();

            var events = entites.
                SelectMany(e => e.Events)
                .ToList();

            entites.ForEach(e => e.ClearEvents());

            foreach ( var e in events) 
                await _mediator.Publish(e);
        }
    }
}
