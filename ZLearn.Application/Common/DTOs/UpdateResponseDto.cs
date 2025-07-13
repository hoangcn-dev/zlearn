using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Domain.Common;

namespace ZLearn.Application.Common.DTOs
{
    public class UpdateResponseDto
    {
        public string Id { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class UpdateResponseMapping : Profile
    {
        public UpdateResponseMapping()
        {
            CreateMap<AuditableEntity, UpdateResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.LastModifiedAt));
        }
    }
}
