using Microsoft.AspNetCore.Http;
using ZLearn.Application.Files.DTOs;

namespace ZLearn.Application.Files.Commands.SaveFile
{
    public class SaveFilesCommand : IRequest<ListSavedFileDto>
    {
        public List<SaveFileRequestItemDto> Files { get; set; }
    }
}
