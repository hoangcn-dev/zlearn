namespace ZLearn.Application.Auth.DTOs
{
    public class UpdateUserProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public bool IsShowNickName { get; set; }
        public string? ImageId { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
