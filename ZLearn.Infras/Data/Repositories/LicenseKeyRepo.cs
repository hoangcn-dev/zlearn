using ZLearn.Application.LicenseKeys;
using ZLearn.Domain.Entities;

namespace ZLearn.Infras.Data.Repositories
{
    public class LicenseKeyRepo : BaseRepo<LicenseKey>, ILicenseKeyRepo
    {
        public LicenseKeyRepo(AppDbContext context) : base(context)
        {
        }
    }
}
