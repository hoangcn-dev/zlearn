using Zlearn.V2.Domain.CatalogContext.Categories.Events;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Categories
{
    public class Category : AggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }

        public Category() { }

        public Category(
            string categoryId,
            string name,
            string slug,
            string? description,
            string? thumbnailUrl)
        {
            Id = categoryId; 
            Name = name;
            Slug = slug;
            Description = description;
            ThumbnailUrl = thumbnailUrl;

            RaiseEvent(new CategoryCreatedEvent(
                Id,
                Name,
                Slug,
                Description,
                ThumbnailUrl
            ));
        }

        public void Update(
            string name,
            string slug,
            string? description,
            string? thumbnailUrl) 
        {
            Name = name;
            Slug = slug;
            Description = description;
            ThumbnailUrl = thumbnailUrl;

            RaiseEvent(new CategoryUpdatedEvent(
                Id,
                Name,
                Slug,
                Description,
                ThumbnailUrl
            ));
        }

        public void Delete()
        {
            RaiseEvent(new CategoryDeletedEvent(Id));
        }
    }
}
