using System.Collections.Generic;
using Zlearn.V2.Domain.CatalogContext.Quizzes;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Tags
{
    public class Tag : AuditableEntity
    {
        public Tag() {}
        public Tag(string name, string? id = null)
        {
            Id = string.IsNullOrEmpty(id) ? IdGenerator.Generate("TAG") : id;
            Name = name;
            RaiseEvent(new Events.TagCreatedEvent(Id, Name));
        }

        public string Name { get; set; } = string.Empty;
        public List<Quiz> Quizzes { get; set; } = new();

        public void Delete()
        {
            RaiseEvent(new Events.TagDeletedEvent(Id, Name));
        }
    }
}
