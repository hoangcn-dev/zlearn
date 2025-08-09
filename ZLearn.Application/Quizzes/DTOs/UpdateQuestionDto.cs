using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class UpdateQuestionDto
    {
        public string? Id { get; set; }
        public string? StringContent { get; set; }
        public string Slug { get; set; }
        public List<string> MediaFileUrls { get; set; } = new List<string>();
        public int CorrectKey { get; set; }
        public int Order { get; set; }
        public List<UpdateAnswerDto> Answers { get; set; }
    }

    public class UpdateQuestionMapping : Profile
    {
        public UpdateQuestionMapping()
        {
            CreateMap<Question, UpdateQuestionDto>()
            .ForMember(dest => dest.MediaFileUrls, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.MediaFileUrls) ? new List<string>() : src.MediaFileUrls.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()));
        }
    }
}
