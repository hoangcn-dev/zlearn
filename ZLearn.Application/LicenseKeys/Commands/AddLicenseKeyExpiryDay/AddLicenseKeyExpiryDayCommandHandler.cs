using ZLearn.Application.Common.Commands;
using ZLearn.Application.LicenseKeys;
using ZLearn.Domain.Entities;
namespace AddLicenseKeyExpiryDay
{
    public class AddLicenseKeyExpiryDayCommandHandler : BaseCommandHandler, IRequestHandler<AddLicenseKeyExpiryDayCommand, string>
    {
        private readonly ILicenseKeyRepo _licenseKeyRepo;

        public AddLicenseKeyExpiryDayCommandHandler(
            IMapper mapper,
            IMediator mediator,
            ILicenseKeyRepo licenseKeyRepo) : base(mapper, mediator)
        {
            _licenseKeyRepo = licenseKeyRepo;
        }
        public async Task<string> Handle(AddLicenseKeyExpiryDayCommand request, CancellationToken cancellationToken)
        {
            var licenseKey = await _licenseKeyRepo.Get(request.Id)
                ?? throw new NotFoundException("The license does not exist");

            if (licenseKey.Status == LicenseKeyStatus.Revoked)
                throw new BadRequestException("The license key has been revoked.");

            licenseKey.LifeDays += request.AddDays;
            _licenseKeyRepo.Update(licenseKey);
            await _licenseKeyRepo.SaveChanges();

            return licenseKey.Id;
        }
    }
}
