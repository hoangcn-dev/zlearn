using MediatR;
using Microsoft.AspNetCore.Http;
using Zlearn.V2.Application.Files.DTOs;

namespace Zlearn.V2.Application.Files.Commands.SaveFile
{
    public class SaveFilesCommand : IRequest<ListSavedFileDto>
    {
        public List<SaveFileRequestItemDto> Files { get; set; }
    }
}

