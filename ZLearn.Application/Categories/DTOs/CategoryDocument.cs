using System;

namespace ZLearn.Application.Categories.DTOs
{
    public class CategoryDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ParentId { get; set; }
        public DateTimeOffset SyncedAt { get; set; }
    }
}
