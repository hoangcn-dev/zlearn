using ZLearn.Domain.Common;
using ZLearn.Domain.Entities;

namespace ZLearn.Domain.Events.Cate
{
    public class CateUpdatedEvent : BaseEvent
    {
        public CateUpdatedEvent(Category category)
        {
            Category = category;
        }

        public Category Category { get; }
    }
}
