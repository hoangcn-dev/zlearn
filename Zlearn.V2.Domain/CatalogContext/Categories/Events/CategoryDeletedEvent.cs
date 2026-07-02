using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.CatalogContext.Categories.Events
{
    public record CategoryDeletedEvent : DeletedEvent
    {
        public CategoryDeletedEvent(string Id) : base(Id)
        {
        }
    }
}
