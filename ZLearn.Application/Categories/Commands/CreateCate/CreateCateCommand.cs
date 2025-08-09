using Microsoft.AspNetCore.Http;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Categories.Commands.CreateCate
{
    public class CreateCateCommand : IRequest<CreateResponseDto>
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
