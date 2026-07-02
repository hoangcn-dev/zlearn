using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Zlearn.V2.Application.Categories.Commands.CreateCategory;
using Zlearn.V2.Application.Categories.Queries.GetCategoryById;
using Zlearn.V2.Application.Categories.Queries.GetAllCategories;
using Zlearn.V2.Application.Categories.DTOs;
using Zlearn.V2.Application.Common.DTOs;
using CreateResponseDto = Zlearn.V2.Application.Common.DTOs.CreateResponseDto;

namespace ZLearn.Web.Controllers.API.V2
{
    [Route("api/v2/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
        {
            var res = await _mediator.Send(command);
            return Ok(Result<CreateResponseDto>.Success("Create category v2 successfully.", res));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(string id)
        {
            var res = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
            return Ok(Result<CateDetailDto>.Success("Get category v2 successfully.", res));
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            var res = await _mediator.Send(new GetCategoryByIdQuery { Slug = slug });
            return Ok(Result<CateDetailDto>.Success("Get category v2 by slug successfully.", res));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var res = await _mediator.Send(new GetAllCategoriesQuery());
            return Ok(Result<System.Collections.Generic.IEnumerable<CateListItemDto>>.Success("Get all categories v2 successfully.", res));
        }
    }
}
