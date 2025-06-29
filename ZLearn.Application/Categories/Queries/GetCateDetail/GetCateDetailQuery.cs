using ZLearn.Application.Categories.DTOs;

namespace ZLearn.Application.Categories.Queries.GetCateDetail
{
    public class GetCateDetailQuery : IRequest<CateDetailDto>
    {
        public string CateId { get; set; }
    }
}
