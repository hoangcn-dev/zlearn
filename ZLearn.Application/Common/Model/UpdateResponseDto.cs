using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Common.Model
{
    public class UpdateResponseDto
    {
        public string Id { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
