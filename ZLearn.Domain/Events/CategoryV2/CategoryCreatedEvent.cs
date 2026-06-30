using System;
using ZLearn.Domain.Common;

namespace ZLearn.Domain.Events.CategoryV2
{
    public class CategoryCreatedEvent : IDomainEvent
    {
        public string CategoryId { get; }
        public string Name { get; }
        public string Slug { get; }
        public DateTimeOffset OccurredOn { get; }

        public CategoryCreatedEvent(string categoryId, string name, string slug)
        {
            CategoryId = categoryId;
            Name = name;
            Slug = slug;
            OccurredOn = DateTimeOffset.UtcNow;
        }
    }
}
