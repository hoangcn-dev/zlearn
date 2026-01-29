using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Categories.Commands.UpdateCate
{
    public class UpdateCateCommand : IRequest<UpdateResponseDto>
    {
        public string? CateId { get; set; }
        public UpdateCateDto Data { get; set; }
    }
}
