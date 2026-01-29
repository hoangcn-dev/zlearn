using ZLearn.Application.Auth.DTOs;

namespace ZLearn.Application.Auth.Queries.GetUserDetail
{
    public class GetUserDetailQuery : IRequest<UserDetailDto>
    {
        public string Id { get; set; }
    }
}
