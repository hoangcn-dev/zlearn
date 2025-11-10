using ZLearn.Application.LicenseKeys.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.LicenseKeys.Queries.CheckLicenseKey
{
    public class CheckLicenseKeyQuery : IRequest<LicenseKeyInfoDto>
    {
        public string Key { get; set; }
    }
}
