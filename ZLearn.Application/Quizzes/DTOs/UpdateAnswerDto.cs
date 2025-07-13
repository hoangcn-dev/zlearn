using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class UpdateAnswerDto
    {
        public string? Id { get; set; }
        public int Key { get; set; }
        public string? StringContent { get; set; }
        public List<string> ImageIds { get; set; } = new List<string>();
    }

    public class UpdateAnswerMapping : Profile
    {
        public UpdateAnswerMapping()
        {
            CreateMap<Answer, UpdateAnswerDto>()
                .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.ImageIds) ? new List<string>() : src.ImageIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()));
                //.ReverseMap()
                //.ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src =>
                //    src.ImageIds == null || !src.ImageIds.Any() ? null : string.Join(",", src.ImageIds)));
        }
    }
}