using HUP.Core.Entities.Shared;
using HUP.Core.Entities.Academics;
using HUP.Core.Enums;


namespace HUP.Core.Entities.Identity
{
    public class User : BaseEntity
    {
        public string NationalId { get; set; }
        public string? Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime PasswordExpiryDate { get; set; }
        public string FullName { get; set; }
        public Guid RoleId { get; set; }
        public Role UserRole { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;

        public UserPersonalInfo PersonalInfo { get; set; }
        public UserContact ContactInfo { get; set; }
        public ICollection<Role> CreatedRoles { get; set; } = new List<Role>();

    }
}