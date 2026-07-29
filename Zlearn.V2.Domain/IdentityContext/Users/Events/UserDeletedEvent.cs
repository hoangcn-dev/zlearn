using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.IdentityContext.Users.Events
{
    public record UserDeletedEvent : DeletedEvent
    {
        public UserDeletedEvent(string Id) : base(Id)
        {
        }
    }
}
