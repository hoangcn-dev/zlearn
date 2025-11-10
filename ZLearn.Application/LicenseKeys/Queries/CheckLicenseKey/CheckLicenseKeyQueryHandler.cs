using ZLearn.Application.Common.Queries;
using ZLearn.Application.LicenseKeys.DTOs;
using ZLearn.Domain.Entities;
namespace ZLearn.Application.LicenseKeys.Queries.CheckLicenseKey
{
    public class CheckLicenseKeyQueryHandler : BaseQueryHandler, IRequestHandler<CheckLicenseKeyQuery, LicenseKeyInfoDto>
    {
        private readonly ILicenseKeyRepo _licenseKeyRepo;

        public CheckLicenseKeyQueryHandler(
            IMapper mapper,
            IMediator mediator,
            ILicenseKeyRepo licenseKeyRepo) : base(mapper, mediator)
        {
            _licenseKeyRepo = licenseKeyRepo;
        }

        public async Task<LicenseKeyInfoDto> Handle(CheckLicenseKeyQuery request, CancellationToken cancellationToken)
        {
            var licenseKey = await _licenseKeyRepo.Get(l => l.Key == request.Key)
                ?? throw new NotFoundException("The license does not exist");

            return new LicenseKeyInfoDto
            {
                Id = licenseKey.Id,
                Name = licenseKey.Name,
                CreatedAt = licenseKey.CreatedAt.DateTime,
                HardwareId = licenseKey.HardwareId,
                Key = licenseKey.Key,
                Level = licenseKey.Level,
                LifeDays = licenseKey.LifeDays,
                Status = licenseKey.Status,
                Type = licenseKey.Type,
            };
        }
    }
}
