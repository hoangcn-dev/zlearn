using ZLearn.Domain.Common;
using ZLearn.Domain.Entities;

namespace ZLearn.Domain.Events.Cate
{
    public class CateCreatedEvent : BaseEvent
    {
        public CateCreatedEvent(Category category)
        {
            Category = category;
        }

        public Category Category { get; }
    }
}
