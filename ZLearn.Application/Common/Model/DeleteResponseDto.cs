using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Common.Model
{
    public class DeleteResponseDto
    {
        public IEnumerable<string> DeletedIds { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
    }
}
