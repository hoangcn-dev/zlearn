using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZLearn.Application.Auth.Commands.GetAuthData;
using ZLearn.Application.Auth.Commands.GoogleSignIn;
using ZLearn.Application.Auth.Commands.SignIn;
using ZLearn.Application.Auth.Commands.UpdateUser;
using ZLearn.Application.Auth.Commands.UpdateUserProfile;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Auth.Queries.GetAllRoles;
using ZLearn.Application.Auth.Queries.GetListUsers;
using ZLearn.Application.Auth.Queries.GetUserDetail;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Utils;

namespace ZLearn.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public AuthController(
            IMediator mediator,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("Test")]
        [Authorize(Policy = "OnlyAdmin")]
        public IActionResult Test()
        {
            return Ok("AuthController is working!");
        }


        #region Users
        [HttpGet("roles")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> GetAllRoles()
        {
            var res = await _mediator.Send(new GetAllRolesQuery());
            return Ok(Result<List<string>>.Success("Get list roles successfully.", res));
        }

        [HttpGet("users")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> GetListUsers([FromQuery] GetListUsersQuery query)
        {
            var res = await _mediator.Send(query);
            return Ok(Result<PaginatedDto<UserListItemDto>>.Success("Get list users successfully.", res));
        }

        [HttpGet("users/{id}")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var query = new GetUserDetailQuery { Id = id };
            var res = await _mediator.Send(query);
            return Ok(Result<UserDetailDto>.Success("Get user successfully.", res));
        }

        [HttpPost("users/{id}")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateContentDto data)
        {
            var command = new UpdateUserConmand
            {
                Id = id,
                UpdateData = data
            };
            var res = await _mediator.Send(command);
            return Ok(Result<UpdateResponseDto>.Success("Update user successfully.", res));
        }

        [HttpPost("user-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDto data)
        {
            var command = new UpdateUserProfileCommand
            {
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                UpdateData = data
            };
            var res = await _mediator.Send(command);
            return Ok(Result<UpdateResponseDto>.Success("Update profile successfully.", res));
        }
        #endregion


        #region Authentication
        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInCommand data)
        {
            var res = await _mediator.Send(data);
            _logger.LogInformation("Signed in successfully.");
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
            var command = new GetAuthDataQuery
            {
                Claims = User
            };
            var res = await _mediator.Send(command);
            return Ok(Result<UserSessionDataDto>.Success("Get auth data successfully", res));
        }

        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOut()
        {
            Response.Cookies.Delete("token");
            return Ok(Result<NoData>.Success("Signed out successfully."));
        } 
        #endregion
    }
}
