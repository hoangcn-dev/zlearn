using AutoMapper;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Categories.DTOs
{
    public class CateListItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int QuizCount { get; set; }
        public string Slug { get; set; }
        public int AttemptCount { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Category, CateListItemDto>()
                    .ForMember(c => c.QuizCount, opt => opt.MapFrom(e => e.Quizzes.Count));
            }
        }
    }
}
