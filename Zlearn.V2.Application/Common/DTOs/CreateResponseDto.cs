using AutoMapper;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Application.Common.DTOs
{
    public class CreateResponseDto
    {
        public string Id { get; set; } = string.Empty;
    }

    public class CreateResponseMapping : Profile
    {
        public CreateResponseMapping()
        {
            CreateMap<BaseEntity, CreateResponseDto>();
        }
    }
}
