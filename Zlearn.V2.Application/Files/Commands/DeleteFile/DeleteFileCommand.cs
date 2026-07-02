using System.Security.Claims;
using MediatR;
using Zlearn.V2.Application.Common.DTOs;

namespace Zlearn.V2.Application.Files.Commands.DeleteFile
{
    public class DeleteFileCommand : IRequest<DeleteResponseDto>
    {
        public ClaimsPrincipal Claims { get; set; }
        public List<string> FileIds { get; set; }
    }
}

