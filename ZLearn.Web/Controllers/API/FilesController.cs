using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Files.Commands.DeleteFile;
using ZLearn.Application.Files.Commands.SaveFile;
using ZLearn.Application.Files.DTOs;

namespace ZLearn.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FilesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> SaveFiles([FromForm] SaveFilesCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(Result<ListSavedFileDto>.Success("Save files successfully.", result));
        }


        [HttpPost("delete")]
        public async Task<IActionResult> DeleteFiles([FromBody] DeleteFileRequestDto data)
        {
            var result = await _mediator.Send(new DeleteFileCommand
            {
                Claims = User,
                FileIds = data.FileIds
            });
            return Ok(Result<DeleteResponseDto>.Success("Delete files successfully.", result));
        }
    }
}
