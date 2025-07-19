using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Domain.Common;

namespace ZLearn.Infras.Data.Interceptors
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
            var userId = _contextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
            foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State is EntityState.Added or EntityState.Modified ||
                    entry.HasChangedOwnedEntities())
                {
                    var current = DateTimeOffset.UtcNow;
                    if (entry.State is EntityState.Added)
                    {
                        entry.Entity.CreatedBy = userId;
                        entry.Entity.CreatedAt = current;
                    }
                    entry.Entity.ModifiedBy = userId;
                    entry.Entity.LastModifiedAt = current;
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
