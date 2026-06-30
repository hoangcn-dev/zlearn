using System;

namespace Zlearn.V2.Domain.Common
{
    public interface ICreatedAuditEvent
    {
        string CreatedBy { get; set; }
        DateTimeOffset CreatedAt { get; set; }
    }
}
