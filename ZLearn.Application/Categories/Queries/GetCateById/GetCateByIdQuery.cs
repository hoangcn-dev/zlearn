using ZLearn.Application.Categories.DTOs;

namespace ZLearn.Application.Categories.Queries.GetCateById 
{
    public class GetCateByIdQuery : IRequest<CateDetailDto>
    {
        public string? Id { get; set; }
        public string? Slug { get; set; }
    }
}
