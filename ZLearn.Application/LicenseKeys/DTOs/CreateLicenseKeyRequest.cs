using ZLearn.Domain.Entities;

namespace ZLearn.Application.LicenseKeys.DTOs
{
    public class CreateLicenseKeyRequest
    {
        public int Count { get; set; }
        public int LifeDays { get; set; }
        public LicenseKeyType Type { get; set; }
        public LicenseKeyLevel Level { get; set; }
    }
}
