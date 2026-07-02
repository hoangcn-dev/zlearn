using AutoMapper;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Application.Common.DTOs
{
    public class UpdateResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class UpdateResponseMapping : Profile
    {
        public UpdateResponseMapping()
        {
            CreateMap<BaseEntity, UpdateResponseDto>();
        }
    }
}

