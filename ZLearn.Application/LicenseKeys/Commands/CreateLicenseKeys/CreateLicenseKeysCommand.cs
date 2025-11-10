using ZLearn.Application.LicenseKeys.DTOs;

namespace ZLearn.Application.LicenseKeys.Commands.CreateLicenseKeys
{
    public class CreateLicenseKeysCommand : IRequest<List<string>>
    {
        public CreateLicenseKeyRequest Data { get; set; }
    }
}
