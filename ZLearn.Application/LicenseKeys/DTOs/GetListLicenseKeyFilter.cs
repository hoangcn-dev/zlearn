using ZLearn.Application.Common.DTOs;
using ZLearn.Domain.Entities;

namespace LicenseKeyManage.Models
{
    public class GetListLicenseKeyFilter : PagingRequestDto
    {
        public LicenseKeyType Type { get; set; } = LicenseKeyType.FlowVeoAutoTool;
        public LicenseKeyStatus? Status { get; set; }
        public LicenseKeyLevel? Level { get; set; }
    }
}
