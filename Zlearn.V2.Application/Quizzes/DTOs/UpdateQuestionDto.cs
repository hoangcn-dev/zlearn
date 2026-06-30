using AutoMapper;
using System;
using System.Linq;

namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class UpdateQuestionDto
    {
        public string? Id { get; set; }
        public string? StringContent { get; set; }
        public string Slug { get; set; } = string.Empty;
        public List<string> MediaFileUrls { get; set; } = new List<string>();
        public string? Explanation { get; set; }
        public int Order { get; set; }
        public List<UpdateAnswerDto> Answers { get; set; } = new List<UpdateAnswerDto>();
    }

    public class UpdateQuestionMapping : Profile
    {
        public UpdateQuestionMapping()
        {
            CreateMap<QuestionDocumentItem, UpdateQuestionDto>()
            .ForMember(dest => dest.MediaFileUrls, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.MediaFileUrls) ? new List<string>() : src.MediaFileUrls.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()));
        }
    }
}
