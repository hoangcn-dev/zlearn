using ZLearn.Application.Common.Model;

namespace ZLearn.Application.Categories.Commands.UpdateCate
{
    public class UpdateCateCommand : IRequest<UpdateResponseDto>
    {
        public string? CateId { get; set; }
        public string Name { get; set; }
    }
}
