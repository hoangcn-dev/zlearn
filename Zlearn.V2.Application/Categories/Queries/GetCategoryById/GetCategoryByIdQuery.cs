using MediatR;
using ZLearn.Application.Categories.DTOs;

namespace Zlearn.V2.Application.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdQuery : IRequest<CateDetailDto>
    {
        public string? Id { get; set; }
        public string? Slug { get; set; }
    }
}
