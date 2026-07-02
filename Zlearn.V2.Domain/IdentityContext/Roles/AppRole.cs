using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.IdentityContext.Roles
{
    public class AppRole : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? NormalizedName { get; set; }
    }
}
