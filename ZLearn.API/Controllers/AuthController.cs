using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using ZLearn.Application.Auth.Commands.GetAuthData;
using ZLearn.Application.Auth.Commands.GoogleSignIn;
using ZLearn.Application.Auth.Commands.SignIn;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Utils;
using ZLearn.Infras.Identity;

namespace ZLearn.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public AuthController(
            IMediator mediator, 
            IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
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

        [HttpGet("sign-in/google")]
        public IActionResult SignInWithGoogle([FromQuery] string returnUrl)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(SignInWithGoogleCallback), new { returnUrl }),
                Items = {
                    { "prompt", "select_account" }
                }
            };
            return Challenge(props, "Google");
        }

        [HttpGet("sign-in/google/callback")]
        public async Task<IActionResult> SignInWithGoogleCallback([FromQuery] string returnUrl)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("Google");
            var sessionData = await _mediator.Send(new GoogleSignInCommand { AuthenticateResult = authenticateResult });
            if (sessionData == null)
                return Redirect(StringHelper.AppendParamsToUrl(returnUrl, new Dictionary<string, string>
                {
                    { "google-login-success", "false" }
                }));
            var cookieOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                MaxAge = TimeSpan.FromMinutes(double.Parse(_configuration.GetSection("JWT")["ATExpirationMinutes"]))
            };
            Response.Cookies.Append("token", sessionData.Token.AccessToken, cookieOptions);
            return Redirect(StringHelper.AppendParamsToUrl(returnUrl, new Dictionary<string, string>
            {
                { "google-login-success", "true" }
            }));
        }

        [HttpGet("session-data")]
        [Authorize]
        public async Task<IActionResult> GetGoogleAuthResult()
        {
            var command = new GetAuthDataCommand
            {
                Claims = User
            };
            var res = await _mediator.Send(command);
            return Ok(Result<UserSessionDataDto>.Success("Get auth data successfully", res));
        }

        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOut()
        {
            return Ok(Result<NoData>.Success("Signed out successfully."));
        }
    }
}
