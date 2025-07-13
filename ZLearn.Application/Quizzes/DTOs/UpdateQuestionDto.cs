
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
        public List<string> ImageIds { get; set; } = new List<string>();
        public List<string> AudioIds { get; set; } = new List<string>();
        public int CorrectKey { get; set; }
        public int Order { get; set; }
        public List<UpdateAnswerDto> Answers { get; set; }
    }

    public class UpdateQuestionMapping : Profile
    {
        public UpdateQuestionMapping()
        {
            CreateMap<Question, UpdateQuestionDto>()
            .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.ImageIds) ? new List<string>() : src.ImageIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()))
            .ForMember(dest => dest.AudioIds, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.AudioIds) ? new List<string>() : src.AudioIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()));
            //.ReverseMap()
            //.ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src =>
            //    src.ImageIds == null || !src.ImageIds.Any() ? null : string.Join(",", src.ImageIds)))
            //.ForMember(dest => dest.AudioIds, opt => opt.MapFrom(src =>
            //    src.AudioIds == null || !src.AudioIds.Any() ? null : string.Join(",", src.AudioIds)));
        }
    }
}
