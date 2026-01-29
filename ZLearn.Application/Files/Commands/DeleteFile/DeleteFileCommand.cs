using System.Security.Claims;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Files.Commands.DeleteFile
{
    public class DeleteFileCommand : IRequest<DeleteResponseDto>
    {
        public ClaimsPrincipal Claims { get; set; }
        public List<string> FileIds { get; set; }
    }
}
