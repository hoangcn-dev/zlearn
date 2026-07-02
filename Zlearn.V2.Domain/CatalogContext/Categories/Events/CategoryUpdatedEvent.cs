using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Categories.Events
{
    public record CategoryUpdatedEvent(
        string CategoryId,
        string Name,
        string Slug,
        string? Description,
        string? ThumbnailUrl
    ) : DomainEvent, IModifiedAuditEvent
    {
        public string? ModifiedBy { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
        public override string AggregateId => CategoryId;
    }
}
