using ZLearn.Application.Common.Model;

namespace ZLearn.Application.Categories.Commands.CreateCate
{
    public class CreateCateCommand : IRequest<CreateResponseDto>
    {
        public string Name { get; init; }
    }
}
