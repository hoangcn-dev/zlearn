using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlearn.V2.Application.Common.DTOs
{
    public class DeleteRequestDto
    {
        public List<string> Ids { get; set; } = new List<string>();
    }
}
