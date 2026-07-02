namespace Zlearn.V2.Application.Identity.DTOs
{
    public class UserDetailDto : UserListItemDto
    {
        public bool IsShowNickName { get; set; }
        public string ImagePath { get; set; }
        public string PhoneNumber { get; set; }
    }
}
