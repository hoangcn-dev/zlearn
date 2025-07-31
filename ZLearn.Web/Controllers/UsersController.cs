using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using ZLearn.Application.Auth.Commands.UpdateUserProfile;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Auth.Queries.GetUserDetail;

namespace ZLearn.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var profile = await _mediator.Send(new GetUserDetailQuery
            {
                Id = userId
            });
            return View(profile);
        }
    }
}
