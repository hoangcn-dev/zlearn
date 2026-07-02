namespace Zlearn.V2.Application.Common.DTOs
{
    public class PagingRequestDto
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string? OrderBy { get; set; }
    }
}
