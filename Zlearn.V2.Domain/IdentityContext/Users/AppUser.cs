using Zlearn.V2.Domain.Common;
using Zlearn.V2.Domain.IdentityContext.Roles;
using Zlearn.V2.Domain.IdentityContext.Users.Events;

namespace Zlearn.V2.Domain.IdentityContext.Users
{
    public class AppUser : AuditableEntity
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string NickName { get; set; }
        public bool IsShowNickName { get; set; }
        public DateTimeOffset LastLogin { get; set; }
        public bool IsActive { get; set; }

        public AppUser()
        {

        }

        public AppUser(string userName, string email, string? phoneNumber, string? imageUrl, string? firstName,
            string? lastName, string nickName, bool isShowNickName, DateTimeOffset lastLogin, bool isActive, AppRole role)
        {
            Id = IdGenerator.Generate("ACC");
            UserName = userName;
            Email = email;
            PhoneNumber = phoneNumber;
            ImageUrl = imageUrl;
            FirstName = firstName;
            LastName = lastName;
            NickName = nickName;
            IsShowNickName = isShowNickName;
            LastLogin = lastLogin;
            IsActive = isActive;

            RaiseEvent(new UserCreatedEvent
            {
                Id = Id,
                UserName = userName,
                Email = email,
                PhoneNumber = phoneNumber,
                ImageUrl = imageUrl,
                FirstName = firstName,
                LastName = lastName,
                NickName = nickName,
                IsShowNickName = isShowNickName,
                LastLogin = lastLogin,
                IsActive = isActive,
                RoleId = role.Id,
                RoleName = role.Name
            });
        }

        public void SignIn()
        {
            LastLogin = DateTimeOffset.UtcNow;
            RaiseEvent(new UserSignedInEvent
            {
                Id = Id,
                SignedInAt = LastLogin
            });
        }

        public void SignOut()
        {
            RaiseEvent(new UserLoggedOutEvent
            {
                Id = Id,
                LoggedOutAt = DateTimeOffset.UtcNow
            });
        }

        public void UpdateProfile(string? phoneNumber, string? imageUrl, string? firstName, string? lastName, string nickName,
            bool isShowNickName, bool isActive)
        {
            PhoneNumber = phoneNumber;
            ImageUrl = imageUrl;
            FirstName = firstName;
            LastName = lastName;
            NickName = nickName;
            IsShowNickName = isShowNickName;
            IsActive = isActive;
            RaiseEvent(new UserUpdatedEvent
            {
                Id = Id,
                PhoneNumber = phoneNumber,
                ImageUrl = imageUrl,
                FirstName = firstName,
                LastName = lastName,
                NickName = nickName,
                IsShowNickName = isShowNickName,
                IsActive = isActive
            });
        }

        public void Delete()
        {
            RaiseEvent(new UserDeletedEvent(Id));
        }
    }
}
