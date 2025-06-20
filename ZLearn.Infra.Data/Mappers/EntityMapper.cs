using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Domain.Entities;
using ZLearn.Infra.Data.Entities;

namespace ZLearn.Infra.Data.Mappers
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            //CreateMap<Answer, AnswerEntity>()
            //    .ForMember(ae => ae.QuestionId, opt => opt.MapFrom(a => a.Question.Id))
            //    .ForMember(ae => ae.ImageUrl, opt => opt.MapFrom(a => a.Image != null ? a.Image.Url : null));
            //CreateMap<Question, QuestionEntity>()
            //    .ForMember(qe => qe.ImageUrl, opt => opt.MapFrom(q => q.Image != null ? q.Image.Url : null))
            //    .ForMember(qe => qe.AudioUrl, opt => opt.MapFrom(q => q.Audio != null ? q.Audio.Url : null));
            //CreateMap<Tag, TagEntity>();
            //CreateMap<Tag, TagEntity>();
            //CreateMap<Quiz, QuizEntity>()
            //    .ForMember(qe => qe.QuizTags, opt => opt.MapFrom(q => q.Tags.Select(t => new QuizTagEntity {
            //        QuizId = q.Id,
            //        TagId = t.Id
            //    })));
            CreateMap<Category, CategoryEntity>()
                .ForMember(ce => ce.Quizzes, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
