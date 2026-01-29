using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class UpdateQuizDto
    {
        public string? Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string CategoryId { get; set; }
        public List<string> Tags { get; set; }
        public List<UpdateQuestionDto> Questions { get; set; }
    }

    public class UpdateQuizMapping : Profile
    {
        public UpdateQuizMapping()
        {
            CreateMap<Quiz, UpdateQuizDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => t.Name)));
                //.ReverseMap()
                //.ForMember(dest => dest.Tags, opt => opt.Ignore()); // Tags will be handled separately in the command handler
        }
    }
}
