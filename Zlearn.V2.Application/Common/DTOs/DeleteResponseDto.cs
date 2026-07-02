using System;
using System.Collections.Generic;

namespace Zlearn.V2.Application.Common.DTOs
{
    public class DeleteResponseDto
    {
        public IEnumerable<string> DeletedIds { get; set; } = new List<string>();
        public DateTimeOffset DeletedAt { get; set; }
    }
}
