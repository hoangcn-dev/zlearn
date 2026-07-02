using MediatR;
using Zlearn.V2.Application.Common.DTOs;

namespace Zlearn.V2.Application.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommand : IRequest<CreateResponseDto>
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
    }
}


