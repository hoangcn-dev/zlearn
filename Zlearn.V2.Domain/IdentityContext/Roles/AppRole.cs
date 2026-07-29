using Zlearn.V2.Domain.Common;
using Zlearn.V2.Domain.IdentityContext.Roles.Events;

namespace Zlearn.V2.Domain.IdentityContext.Roles
{
    public class AppRole : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? NormalizedName { get; set; }

        public AppRole()
        {
        }

        public AppRole(string name, string? normalizedName = null)
        {
            Id = IdGenerator.Generate("ROL");
            Name = name;
            NormalizedName = normalizedName;

            RaiseEvent(new RoleCreatedEvent
            {
                Id = Id,
                Name = name,
                NormalizedName = normalizedName
            });
        }

        public void Delete()
        {
            RaiseEvent(new RoleDeletedEvent(Id));
        }
    }
}
