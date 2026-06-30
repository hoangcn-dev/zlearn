using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Tags.Events
{
    public record TagCreatedEvent : DomainEvent
    {
        public string TagId { get; }
        public string Name { get; }

        public TagCreatedEvent(string tagId, string name)
        {
            TagId = tagId;
            Name = name;
        }
    }
}
