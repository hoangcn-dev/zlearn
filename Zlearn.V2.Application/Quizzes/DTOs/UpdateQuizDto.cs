using AutoMapper;

namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class UpdateQuizDto
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public List<UpdateQuestionDto> Questions { get; set; } = new();
    }

    public class UpdateQuizMapping : Profile
    {
        public UpdateQuizMapping()
        {
            CreateMap<QuizDocument, UpdateQuizDto>();
        }
    }
}
