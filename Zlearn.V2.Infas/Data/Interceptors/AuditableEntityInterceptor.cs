using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Infas.Data.Interceptors
{
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public AuditableEntityInterceptor(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateAuditableEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditableEntities(DbContext? context)
        {
            if (context == null) return;
            var userId = _contextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var current = DateTimeOffset.UtcNow;

            // 1. Xử lý cho V2 AuditableEntity
            foreach (var entry in context.ChangeTracker.Entries<Zlearn.V2.Domain.Common.AuditableEntity>())
            {
                if (entry.State is EntityState.Added or EntityState.Modified ||
                    entry.HasChangedOwnedEntities())
                {
                    if (entry.State is EntityState.Added)
                    {
                        entry.Entity.CreatedBy = userId ?? "unknown";
                        entry.Entity.CreatedAt = current;
                    }
                    entry.Entity.ModifiedBy = userId ?? "unknown";
                    entry.Entity.LastModifiedAt = current;

                    // Tự động đồng bộ gán thông tin audit cho các Domain Events tương ứng
                    if (entry.Entity is Zlearn.V2.Domain.Common.AggregateRoot aggregate)
                    {
                        foreach (var @event in aggregate.UncommittedEvents)
                        {
                            if (@event is Zlearn.V2.Domain.Common.ICreatedAuditEvent createdEvent)
                            {
                                createdEvent.CreatedBy = entry.Entity.CreatedBy;
                                createdEvent.CreatedAt = entry.Entity.CreatedAt;
                            }
                            if (@event is Zlearn.V2.Domain.Common.IModifiedAuditEvent modifiedEvent)
                            {
                                modifiedEvent.ModifiedBy = entry.Entity.ModifiedBy;
                                modifiedEvent.LastModifiedAt = entry.Entity.LastModifiedAt;
                            }
                        }
                    }
                }
            }
        }
    }

    public static class Extensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}
