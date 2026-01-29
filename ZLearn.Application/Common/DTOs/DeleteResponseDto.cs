using ZLearn.Domain.Common;

namespace ZLearn.Application.Common.DTOs
{
    public class DeleteResponseDto
    {
        public IEnumerable<string> DeletedIds { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
    }
}
