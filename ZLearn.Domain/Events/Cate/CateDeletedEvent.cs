using ZLearn.Domain.Common;
using ZLearn.Domain.Entities;

namespace ZLearn.Domain.Events.Cate
{
    public class CateDeletedEvent : BaseEvent
    {
        public CateDeletedEvent(Category category)
        {
            Category = category;
        }

        public Category Category { get; }
    }
}
