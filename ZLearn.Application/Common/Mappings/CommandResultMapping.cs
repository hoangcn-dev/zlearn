using AutoMapper;
using ZLearn.Application.Common.Model;
using ZLearn.Domain.Common;
namespace ZLearn.Application.Common.Mappings
{
    public class CommandResultMapping : Profile
    {
        public CommandResultMapping()
        {
            CreateMap<BaseEntity, CreateResponseDto>();
        }
    }
}
