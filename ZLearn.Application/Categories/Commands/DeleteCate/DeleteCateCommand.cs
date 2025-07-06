using ZLearn.Application.Common.Model;

namespace ZLearn.Application.Categories.Commands.DeleteCate
{
    public class DeleteCateCommand : IRequest<DeleteResponseDto>
    {
        public List<string> CateIds { get; set; }
    }
}
