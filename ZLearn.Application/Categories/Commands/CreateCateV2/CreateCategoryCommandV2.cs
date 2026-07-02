using MediatR;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Categories.Commands.CreateCateV2
{
    public class CreateCategoryCommandV2 : IRequest<CreateResponseDto>
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
    }
}
