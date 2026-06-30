using System;

namespace Zlearn.V2.Application.Categories.DTOs
{
    public class CategoryDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int QuizCount { get; set; }
        public string? ParentId { get; set; }
        public DateTimeOffset SyncedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
