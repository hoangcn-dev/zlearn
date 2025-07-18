
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class UpdateQuestionDto
    {
        public string? Id { get; set; }
        public string? StringContent { get; set; }
        public List<string> MediaFileIds { get; set; } = new List<string>();
        public int CorrectKey { get; set; }
        public int Order { get; set; }
        public List<UpdateAnswerDto> Answers { get; set; }
    }

    public class UpdateQuestionMapping : Profile
    {
        public UpdateQuestionMapping()
        {
            CreateMap<Question, UpdateQuestionDto>()
            .ForMember(dest => dest.MediaFileIds, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.MediaFileIds) ? new List<string>() : src.MediaFileIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()));
        }
    }
}
