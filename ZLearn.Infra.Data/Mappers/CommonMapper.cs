using AutoMapper;
using ZLearn.Application.Temp.DTOs;
using ZLearn.Infra.Data.Entities;

namespace ZLearn.Application.Mappers
{
    public class CommonMapper : Profile
    {
        public CommonMapper()
        {
            CreateMap<BaseEntity, CreateResultDTO>()
                .ForMember(dto => dto.CreatedAt, opt => opt.MapFrom(be => be.CreatedAt));
            CreateMap<BaseEntity, UpdateResultDTO>()
                .ForMember(dto => dto.UpdatedAt, opt => opt.MapFrom(be => be.UpdatedAt));
            CreateMap<BaseEntity, DeleteResultDTO>()
                .ForMember(dto => dto.DeletedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
