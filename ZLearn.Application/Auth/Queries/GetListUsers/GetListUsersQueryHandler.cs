using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Identity;

namespace ZLearn.Application.Auth.Queries.GetListUsers
{
    public class GetListUsersQueryHandler : IRequestHandler<GetListUsersQuery, PaginatedDto<UserListItemDto>>
    {
        private readonly IIdentityService _identityService;

        public GetListUsersQueryHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<PaginatedDto<UserListItemDto>> Handle(GetListUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _identityService.GetAllUsers(request);
            return users;
        }
    }
}
