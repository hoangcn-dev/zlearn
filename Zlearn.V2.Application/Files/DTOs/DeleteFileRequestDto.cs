using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zlearn.V2.Application.Files.DTOs
{
    public class DeleteFileRequestDto
    {
        public List<string> FileIds { get; set; }
    }
}
