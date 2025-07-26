using ZLearn.Domain.Common;

namespace ZLearn.Domain.Entities
{
    public class AccessHistory : BaseEntity
    {
        public DateOnly Day { get; set; }
        public long AccessCount { get; set; }
    }
}
