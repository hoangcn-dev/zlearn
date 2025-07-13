using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Files.Commands.DeleteFile
{
    public class DeleteFileCommand : IRequest<DeleteResponseDto>
    {
        public List<string> FileIds { get; set; }
    }
}
