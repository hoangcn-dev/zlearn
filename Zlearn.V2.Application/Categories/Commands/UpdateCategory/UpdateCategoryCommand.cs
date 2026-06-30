using MediatR;
using Zlearn.V2.Application.Common.DTOs;

namespace Zlearn.V2.Application.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(
        string Id,
        string Name,
        string? Description,
        string? ThumbnailUrl
    ) : IRequest<CreateResponseDto>;
}
