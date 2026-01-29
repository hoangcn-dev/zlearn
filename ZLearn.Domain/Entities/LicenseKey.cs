using ZLearn.Domain.Common;

namespace ZLearn.Domain.Entities
{
    public enum LicenseKeyType
    {
        FlowVeoAutoTool
    }

    public enum LicenseKeyStatus
    {
        Pending, // Chờ được kích hoạt để set HardwareId
        Active,
        Revoked
    }

    public enum LicenseKeyLevel
    {
        Normal,
        Pro
    }

    public class LicenseKey : AuditableEntity
    {
        public string Name { get; set; }
        public LicenseKeyType Type { get; set; }
        public LicenseKeyStatus Status { get; set; }
        public LicenseKeyLevel Level { get; set; }
        public string Key { get; set; }
        public int LifeDays { get; set; }
        public string? HardwareId { get; set; }
    }
}
