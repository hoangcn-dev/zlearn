using LicenseKeyManage.Models;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.LicenseKeys.DTOs;

namespace ZLearn.Application.LicenseKeys.Queries.GetAllLicenseKeys
{
    public class GetAllLicenseKeysQuery : IRequest<PaginatedDto<LicenseKeyInfoDto>>
    {
        public GetListLicenseKeyFilter Filter { get; set; }
    }
}
