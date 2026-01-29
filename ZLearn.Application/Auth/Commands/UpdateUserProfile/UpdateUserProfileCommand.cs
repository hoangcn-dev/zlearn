using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Auth.Commands.UpdateUserProfile
{
    public class UpdateUserProfileCommand : IRequest<UpdateResponseDto>
    {
        public string Id { get; set; }
        public UpdateUserProfileDto UpdateData { get; set; }
    }
}
