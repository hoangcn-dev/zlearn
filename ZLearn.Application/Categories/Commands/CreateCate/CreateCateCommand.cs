using Microsoft.AspNetCore.Http;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Categories.Commands.CreateCate
{
    public class CreateCateCommand : IRequest<CreateResponseDto>
    {
        public string Name { get; init; }
        public string ThumbnailId { get; init; }
    }
}
