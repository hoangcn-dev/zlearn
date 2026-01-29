using ZLearn.Domain.Common;

namespace ZLearn.Application.Common.DTOs
{
    public class CreateResponseDto
    {
        public string Id { get; set; }
    }

    public class CreateResponseMapping : Profile
    {
        public CreateResponseMapping()
        {
            CreateMap<BaseEntity, CreateResponseDto>();
        }
    }
}
