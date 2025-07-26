using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Auth.Commands.UpdateUser
{
    public class UpdateUserConmand : IRequest<UpdateResponseDto>
    {
        public string Id { get; set; }
        public UserUpdateContentDto UpdateData { get; set; }
    }
}
