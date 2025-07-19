using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLearn.Application.Auth.Commands.SignIn;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.DTOs;

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

        [HttpGet("Test")]
        [Authorize(Policy = "OnlyAdmin")]
        public IActionResult Test()
        {
            return Ok("AuthController is working!");
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInCommand data)
        {
            var res = await _mediator.Send(data);
            return Ok(Result<UserSessionDataDto>.Success("Signed in successfully.", res));
        }

        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOut()
        {
            // Assuming you have a SignOutCommand to handle sign out logic
            // var res = await _mediator.Send(new SignOutCommand());
            // return Ok(Result<NoData>.Success("Signed out successfully."));
            
            // For now, just returning a success message without actual sign-out logic
            return Ok(Result<NoData>.Success("Signed out successfully."));
        }
    }
}
