using MediatR;
using ZLearn.Application.Categories.DTOs;

namespace ZLearn.Application.Categories.Queries.GetCategoryByIdV2
{
    public class GetCategoryByIdQueryV2 : IRequest<CategoryDocument>
    {
        public string? Id { get; set; }
        public string? Slug { get; set; }
    }
}
