using FluentValidation.Results;

namespace ZLearn.Application.Files.DTOs
{
    public class ListSavedFileDto
    {
        public List<SavedFileDto> Files { get; set; }
        public Dictionary<string, string> Errors { get; set; }
    }
}
