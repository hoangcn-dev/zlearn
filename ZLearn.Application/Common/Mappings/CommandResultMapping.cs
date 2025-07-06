using ZLearn.Application.Common.Model;
using ZLearn.Domain.Common;
namespace ZLearn.Application.Common.Mappings
{
    public class CommandResultMapping : Profile
    {
        public CommandResultMapping()
        {
            CreateMap<BaseEntity, CreateResponseDto>();
            CreateMap<AuditableEntity, UpdateResponseDto>()
                .ForMember(r => r.UpdatedAt, opt => opt.MapFrom(e => e.LastModifiedAt));
        }
    }
}
