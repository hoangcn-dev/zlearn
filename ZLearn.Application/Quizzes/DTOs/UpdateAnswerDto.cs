using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class UpdateAnswerDto
    {
        public string? Id { get; set; }
        public int Key { get; set; }
        public string? StringContent { get; set; }
        public List<string> MediaFileUrls { get; set; } = new List<string>();
        public bool IsCorrect { get; set; }
    }

    public class UpdateAnswerMapping : Profile
    {
        public UpdateAnswerMapping()
        {
            CreateMap<Answer, UpdateAnswerDto>()
                .ForMember(dest => dest.MediaFileUrls, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.MediaFileUrls) ? new List<string>() : src.MediaFileUrls.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()));
        }
    }
}