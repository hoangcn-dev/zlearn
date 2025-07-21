using MediatR;
using System.Security.Claims;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.Identity.DTOs;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;

namespace ZLearn.Application.Auth.Commands.GetAuthData
{
    public class GetAuthDataCommandHandler : BaseCommandHandler, IRequestHandler<GetAuthDataCommand, UserSessionDataDto>
    {
        private readonly IFileRepo _fileRepo;

        public GetAuthDataCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IFileRepo fileRepo) : base(mapper, mediator)
        {
            _fileRepo = fileRepo;
        }

        public async Task<UserSessionDataDto> Handle(GetAuthDataCommand request, CancellationToken cancellationToken)
        {
            var imageId = request.Claims.FindFirst("ImageId")!.Value;
            var sessionData = new UserSessionDataDto
            {
                Id = request.Claims.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                UserName = request.Claims.FindFirst("UserName")!.Value,
                LastName = request.Claims.FindFirst("LastName")!.Value,
                FirstName = request.Claims.FindFirst("FirstName")!.Value,
                ImagePath = string.IsNullOrEmpty(imageId) ?
                    StringHelper.GetDefaultImageUrl() :
                    await _fileRepo.Get(imageId, f => f.SourceUrl) ?? StringHelper.GetDefaultImageUrl(),
                Roles = request.Claims.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            };
            return sessionData;
        }
    }
}
