using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.IdentityContext.Roles.Events
{
    public record RoleDeletedEvent : DeletedEvent
    {
        public RoleDeletedEvent(string Id) : base(Id)
        {
        }
    }
}
