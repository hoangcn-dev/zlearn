using System;

namespace Zlearn.V2.Domain.Common
{
    public interface IModifiedAuditEvent
    {
        string? ModifiedBy { get; set; }
        DateTimeOffset? LastModifiedAt { get; set; }
    }
}
