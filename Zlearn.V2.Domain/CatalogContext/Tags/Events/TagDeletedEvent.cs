using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Tags.Events
{
    public record TagDeletedEvent : DeletedEvent
    {
        public string Name { get; }

        public TagDeletedEvent(string tagId, string name) : base(tagId)
        {
            Name = name;
        }
    }
}
