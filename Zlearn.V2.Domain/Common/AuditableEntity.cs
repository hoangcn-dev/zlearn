using System;

namespace Zlearn.V2.Domain.Common
{
    public class AuditableEntity : BaseEntity
    {
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
