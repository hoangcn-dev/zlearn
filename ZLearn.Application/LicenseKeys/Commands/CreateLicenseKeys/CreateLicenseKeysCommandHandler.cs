using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.Utils;
using ZLearn.Domain.Entities;
namespace ZLearn.Application.LicenseKeys.Commands.CreateLicenseKeys
{
    public class CreateLicenseKeysCommandHandler : BaseCommandHandler, IRequestHandler<CreateLicenseKeysCommand, List<string>>
    {
        private readonly ILicenseKeyRepo _licenseKeyRepo;

        public CreateLicenseKeysCommandHandler(
            IMapper mapper,
            IMediator mediator,
            ILicenseKeyRepo licenseKeyRepo) : base(mapper, mediator)
        {
            _licenseKeyRepo = licenseKeyRepo;
        }

        public async Task<List<string>> Handle(CreateLicenseKeysCommand request, CancellationToken cancellationToken)
        {
            var keys = new List<LicenseKey>();
            for (int i = 0; i < request.Data.Count; i++)
            {
                keys.Add(new LicenseKey
                {
                    Id = IdGenerator.Generate("LIC"),
                    Name = StringHelper.GetRandomString(6),
                    Key = Guid.NewGuid().ToString(),
                    Level = request.Data.Level,
                    Status = LicenseKeyStatus.Pending,
                    Type = request.Data.Type,
                    LifeDays = request.Data.LifeDays
                });
            }

            _licenseKeyRepo.CreateRange(keys);
            await _licenseKeyRepo.SaveChanges();

            return keys.Select(l => l.Key).ToList();
        }
    }
}
