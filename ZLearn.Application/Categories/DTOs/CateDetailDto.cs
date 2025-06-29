using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Categories.DTOs
{
    public class CateDetailDto : CateListItemDto
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
