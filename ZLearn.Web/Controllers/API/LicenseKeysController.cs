using AddLicenseKeyExpiryDay;
using LicenseKeyManage.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevokeLicenseKey;
using ValidateLicenseKey;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.LicenseKeys.Commands.CreateLicenseKeys;
using ZLearn.Application.LicenseKeys.DTOs;
using ZLearn.Application.LicenseKeys.Queries.CheckLicenseKey;
using ZLearn.Application.LicenseKeys.Queries.GetAllLicenseKeys;

namespace ZLearn.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseKeysController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LicenseKeysController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateLicenseKeyRequest request)
        {
            var result = await _mediator.Send(new CreateLicenseKeysCommand
            {
                Data = request
            });
            return Ok(Result<List<string>>.Success("Create license keys successfully.", result));
        }

        [HttpPost("revoke")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> Revoke([FromBody] RevokeLicenseKeyCommand data)
        {
            var result = await _mediator.Send(data);
            return Ok(Result<string>.Success("Revoke license key successfully.", result));
        }

        [HttpPost("add-expiry")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> AddExpiry([FromBody] AddLicenseKeyExpiryDayCommand data)
        {
            var result = await _mediator.Send(data);
            return Ok(Result<string>.Success("Add expiry days successfully.", result));
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateLicenseKeyCommand data)
        {
            var result = await _mediator.Send(data);
            return Ok(Result<TimeSpan>.Success("Validate license key successfully.", result));
        }

        [HttpGet]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> GetAll([FromQuery] GetListLicenseKeyFilter filter)
        {
            var result = await _mediator.Send(new GetAllLicenseKeysQuery { Filter = filter });
            return Ok(Result<PaginatedDto<LicenseKeyInfoDto>>.Success("Get license keys successfully.", result));
        }

        [HttpGet("check")]
        [Authorize(Policy = "OnlyAdmin")]
        public async Task<IActionResult> Check([FromQuery] CheckLicenseKeyQuery data)
        {
            var result = await _mediator.Send(data);
            return Ok(Result<LicenseKeyInfoDto>.Success("Check license key successfully.", result));
        }
    }
}
