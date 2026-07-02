namespace Zlearn.V2.Application.Identity.DTOs
{
    public class UserUpdateContentDto : UpdateUserProfileDto
    {
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
    }
}
