using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Categories.Commands.UpdateCate
{
    public class UpdateCateCommand : IRequest<UpdateResponseDto>
    {
        public string? CateId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailId { get; set; }
    }
}
