using ZLearn.Application.Categories.DTOs;

namespace ZLearn.Application.Categories.Queries.GetCateById 
{
    public class GetCateByIdQuery : IRequest<CateDetailDto>
    {
        public string CateId { get; set; }
    }
}
