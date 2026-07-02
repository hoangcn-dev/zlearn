using System;
using System.Collections.Generic;

namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class QuizDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        public int DownloadCount { get; set; }
        public bool IsPublic { get; set; }
        public int QuestionCount { get; set; }
        public int AttemptCount { get; set; }
        public DateTimeOffset SyncedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? LastModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
        public List<QuestionDocumentItem> Questions { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }

    public class QuestionDocumentItem
    {
        public string Id { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? StringContent { get; set; }
        public string MediaFileUrls { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public int Order { get; set; }
        public int AttemptCount { get; set; }
        public List<AnswerDocumentItem> Answers { get; set; } = new();
    }

    public class AnswerDocumentItem
    {
        public string Id { get; set; } = string.Empty;
        public int Key { get; set; }
        public string? StringContent { get; set; }
        public string MediaFileUrls { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class QuizDocumentMapping : AutoMapper.Profile
    {
        public QuizDocumentMapping()
        {
            CreateMap<Zlearn.V2.Domain.CatalogContext.Quizzes.Quiz, QuizDocument>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.CategorySlug, opt => opt.MapFrom(src => src.Category != null ? src.Category.Slug : string.Empty))
                .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Count : 0))
                .ForMember(dest => dest.AttemptCount, opt => opt.MapFrom(src => src.Questions != null ? src.Questions.Sum(q => q.AttemptCount) : 0))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags != null ? src.Tags.Select(t => t.Name).ToList() : new List<string>()));

            CreateMap<Zlearn.V2.Domain.CatalogContext.Questions.Question, QuestionDocumentItem>();
            CreateMap<Zlearn.V2.Domain.CatalogContext.Answers.Answer, AnswerDocumentItem>();
        }
    }
}

