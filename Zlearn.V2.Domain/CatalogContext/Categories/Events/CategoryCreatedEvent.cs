using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Categories.Events
{
    public record CategoryCreatedEvent(
        string CategoryId,
        string Name,
        string Slug,
        string? Description,
        string? ThumbnailUrl
    ) : DomainEvent, ICreatedAuditEvent
    {
        public string CreatedBy { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
