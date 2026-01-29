using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Files.DTOs
{
    public class SavedFileDto
    {
        public string Id { get; set; }
        public string SourceUrl { get; set; }
        public string FileName { get; set; }
        public MediaType Type { get; set; }
        public string Extension { get; set; }
        public double MbSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public double? SecDuration { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class SavedFileMapping : Profile
    {
        public SavedFileMapping()
        {
            CreateMap<MediaFile, SavedFileDto>()
                .ForMember(dest => dest.MbSize, opt => opt.MapFrom(src => src.GetMbSize()));
        }
    }
}
