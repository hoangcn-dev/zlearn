using Microsoft.AspNetCore.Http;

namespace Zlearn.V2.Application.Files.DTOs
{
    public class SaveFileRequestItemDto
    {
        public IFormFile Data { get; set; }
        public string? Name { get; set; }
    }
}
