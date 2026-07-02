using AutoMapper;
using Zlearn.V2.Domain.CatalogContext.Categories;

namespace Zlearn.V2.Application.Categories.DTOs
{
    public class CateListItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int QuizCount { get; set; }
        public string Slug { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
    }
}

