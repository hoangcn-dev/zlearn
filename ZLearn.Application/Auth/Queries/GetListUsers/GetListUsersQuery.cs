using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Auth.Queries.GetListUsers
{
    public class GetListUsersQuery : PagingRequestDto, IRequest<PaginatedDto<UserListItemDto>>
    {
        public string? Email { get; set; }
    }
}
