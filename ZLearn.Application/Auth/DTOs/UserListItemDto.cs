namespace ZLearn.Application.Auth.DTOs
{
    public class UserListItemDto 
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{LastName} {FirstName}";
        public string NickName { get; set; }
        public string Email { get; set; }
        public List<string> Roles  { get; set; }
        public DateTimeOffset LastLogin { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
    }
}
