using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class UpdateAnswerDto
    {
        public string? Id { get; set; }
        public int Key { get; set; }
        public string? StringContent { get; set; }
        public List<string> MediaFileIds { get; set; } = new List<string>();
    }

    public class UpdateAnswerMapping : Profile
    {
        public UpdateAnswerMapping()
        {
            CreateMap<Answer, UpdateAnswerDto>()
                .ForMember(dest => dest.MediaFileIds, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.MediaFileIds) ? new List<string>() : src.MediaFileIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()));
        }
    }
}