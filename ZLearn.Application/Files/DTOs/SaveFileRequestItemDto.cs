using Microsoft.AspNetCore.Http;

namespace ZLearn.Application.Files.DTOs
{
    public class SaveFileRequestItemDto
    {
        public IFormFile Data { get; set; }
        public string? Name { get; set; }
    }
}
