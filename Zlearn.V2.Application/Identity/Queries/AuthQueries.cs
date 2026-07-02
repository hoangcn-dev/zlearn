using System.Collections.Generic;
using System.Security.Claims;
using MediatR;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Identity.DTOs;

namespace Zlearn.V2.Application.Identity.Queries
{
    public class GetAuthDataQuery : IRequest<UserSessionDataDto>
    {
        public ClaimsPrincipal Claims { get; set; } = null!;
    }

    public class GetListUsersQuery : PagingRequestDto, IRequest<PaginatedDto<UserListItemDto>>
    {
        public string? Email { get; set; }
    }

    public class GetUserDetailQuery : IRequest<UserDetailDto>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class GetAllRolesQuery : IRequest<List<string>>
    {
    }
}
