using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.LicenseKeys.DTOs;
using ZLearn.Domain.Entities;
namespace ZLearn.Application.LicenseKeys.Queries.GetAllLicenseKeys
{
    public class GetAllLicenseKeysQueryHandler : BaseQueryHandler, IRequestHandler<GetAllLicenseKeysQuery, PaginatedDto<LicenseKeyInfoDto>>
    {
        private readonly ILicenseKeyRepo _licenseKeyRepo;

        public GetAllLicenseKeysQueryHandler(
            IMapper mapper,
            IMediator mediator,
            ILicenseKeyRepo licenseKeyRepo) : base(mapper, mediator)
        {
            _licenseKeyRepo = licenseKeyRepo;
        }
        public async Task<PaginatedDto<LicenseKeyInfoDto>> Handle(GetAllLicenseKeysQuery request, CancellationToken cancellationToken)
        {
            var filterBuilder = new FilterBuilder<LicenseKey>();
            filterBuilder.AndCondition(l => l.Type == request.Filter.Type);
            if (request.Filter.Status != null)
                filterBuilder.AndCondition(l => l.Status == request.Filter.Status);
            if (request.Filter.Level != null)
                filterBuilder.AndCondition(l => l.Level == request.Filter.Level);

            var licenseKeys = await _licenseKeyRepo.GetPaging(
                page: request.Filter.PageIndex,
                size: request.Filter.PageSize,
                filter: filterBuilder.GetPredicateOrDefault(),
                projector: l => new LicenseKeyInfoDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    CreatedAt = l.CreatedAt.DateTime,
                    HardwareId = l.HardwareId,
                    Key = l.Key,
                    Level = l.Level,
                    LifeDays = l.LifeDays,
                    Status = l.Status,
                    Type = l.Type,
                },
                isAsc: false,
                orderBy: p => p.CreatedAt);
            
            return licenseKeys;
        }
    }
}
