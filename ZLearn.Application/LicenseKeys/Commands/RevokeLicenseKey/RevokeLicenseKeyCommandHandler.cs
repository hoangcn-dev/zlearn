using ZLearn.Application.Common.Commands;
using ZLearn.Application.LicenseKeys;
using ZLearn.Domain.Entities;
namespace RevokeLicenseKey
{
    public class RevokeLicenseKeyCommandHandler : BaseCommandHandler, IRequestHandler<RevokeLicenseKeyCommand, string>
    {
        private readonly ILicenseKeyRepo _licenseKeyRepo;

        public RevokeLicenseKeyCommandHandler(
            IMapper mapper,
            IMediator mediator,
            ILicenseKeyRepo licenseKeyRepo) : base(mapper, mediator)
        {
            _licenseKeyRepo = licenseKeyRepo;
        }
        public async Task<string> Handle(RevokeLicenseKeyCommand request, CancellationToken cancellationToken)
        {
            var licenseKey = await _licenseKeyRepo.Get(request.Id)
                ?? throw new NotFoundException(nameof(LicenseKey));

            if (licenseKey.Status == LicenseKeyStatus.Revoked)
                throw new BadRequestException("The license key has been revoked.");

            licenseKey.Status = LicenseKeyStatus.Revoked;
            _licenseKeyRepo.Update(licenseKey);
            await _licenseKeyRepo.SaveChanges();

            return licenseKey.Id;
        }
    }
}
