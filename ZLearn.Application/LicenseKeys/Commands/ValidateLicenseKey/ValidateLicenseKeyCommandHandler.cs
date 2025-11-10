using ValidateLicenseKey;
using ZLearn.Application.Common.Commands;
using ZLearn.Domain.Entities;
namespace ZLearn.Application.LicenseKeys.Commands.ValidateLicenseKey
{
    public class ValidateLicenseKeyCommandHandler : BaseCommandHandler, IRequestHandler<ValidateLicenseKeyCommand, TimeSpan>
    {
        private readonly ILicenseKeyRepo _licenseKeyRepo;

        public ValidateLicenseKeyCommandHandler(
            IMapper mapper,
            IMediator mediator,
            ILicenseKeyRepo licenseKeyRepo) : base(mapper, mediator)
        {
            _licenseKeyRepo = licenseKeyRepo;
        }

        public async Task<TimeSpan> Handle(ValidateLicenseKeyCommand request, CancellationToken cancellationToken)
        {
            var licenseKey = await _licenseKeyRepo.Get(l => l.Key == request.Key)
                ?? throw new NotFoundException(nameof(LicenseKey));

            if (licenseKey.Status == LicenseKeyStatus.Revoked)
                throw new BadRequestException("The license key has been revoked.");

            var expireAt = licenseKey.CreatedAt.AddDays(licenseKey.LifeDays);

            if (expireAt <= DateTimeOffset.UtcNow)
            {
                licenseKey.Status = LicenseKeyStatus.Revoked;
                _licenseKeyRepo.Update(licenseKey);
                await _licenseKeyRepo.SaveChanges();
                throw new BadRequestException("The license key has expired.");
            }

            if (licenseKey.Status == LicenseKeyStatus.Pending && string.IsNullOrEmpty(licenseKey.HardwareId))
            {
                licenseKey.HardwareId = request.HardwareId;
                licenseKey.Status = LicenseKeyStatus.Active;
                _licenseKeyRepo.Update(licenseKey);
                await _licenseKeyRepo.SaveChanges();
            }

            if (string.Equals(licenseKey.HardwareId, request.HardwareId, StringComparison.OrdinalIgnoreCase)
                && licenseKey.Status == LicenseKeyStatus.Active)
            {
                var remaining = expireAt - DateTimeOffset.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }

            throw new BadRequestException("This license key is already in use by another device.");
        }

    }
}
