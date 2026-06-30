using System.Collections.Generic;

namespace Zlearn.V2.Application.Common.DTOs
{
    public class PaginatedDto<TItem>
    {
        public int TotalItems { get; set; }
        public int TotalPages => TotalItems / PageSize + (TotalItems % PageSize > 0 ? 1 : 0);
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<TItem> Items { get; set; } = new List<TItem>();
    }
}
