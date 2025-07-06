using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLearn.Application.Auth.Commands.SignIn;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.Model;

namespace ZLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInCommand data)
        {
            var res = await _mediator.Send(data);
            return Ok(Result<UserSessionDataDto>.Success("Signed in successfully.", res));
        }
    }
}
