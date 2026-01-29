using System.Security.Claims;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;

namespace ZLearn.Application.Auth.Commands.GetAuthData
{
    public class GetAuthDataQueryHandler : BaseQueryHandler, IRequestHandler<GetAuthDataQuery, UserSessionDataDto>
    {
        private readonly IFileRepo _fileRepo;

        public GetAuthDataQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IFileRepo fileRepo) : base(mapper, mediator)
        {
            _fileRepo = fileRepo;
        }

        public async Task<UserSessionDataDto> Handle(GetAuthDataQuery request, CancellationToken cancellationToken)
        {
            var imageId = request.Claims.FindFirst("ImageUrl")!.Value;
            var sessionData = new UserSessionDataDto
            {
                Id = request.Claims.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                UserName = request.Claims.FindFirst("UserName")!.Value,
                LastName = request.Claims.FindFirst("LastName")!.Value,
                FirstName = request.Claims.FindFirst("FirstName")!.Value,
                ImagePath = request.Claims.FindFirst("ImageUrl")!.Value,
                Roles = request.Claims.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            };
            return sessionData;
        }
    }
}
