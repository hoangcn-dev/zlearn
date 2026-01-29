namespace ZLearn.Application.Auth.DTOs
{
    public class UserDetailDto : UserListItemDto
    {
        public bool IsShowNickName { get; set; }
        public string ImagePath { get; set; }
        public string PhoneNumber { get; set; }
    }
}
